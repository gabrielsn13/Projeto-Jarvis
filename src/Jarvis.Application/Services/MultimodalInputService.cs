using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Microsoft.Extensions.Logging;

namespace Jarvis.Application.Services;

public sealed class MultimodalInputService(
    IInputHandlingService inputHandlingService,
    ISpeechToTextService speechToTextService,
    ITextToSpeechService textToSpeechService,
    IVoiceModeState voiceModeState,
    ILogger<MultimodalInputService> logger) : IMultimodalInputService
{
    public async Task<MultimodalInputResult> HandleAsync(string sessionId, string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A sessão é obrigatória.", nameof(sessionId));
        }

        return await HandleCoreAsync(sessionId, input, cancellationToken);
    }

    private async Task<MultimodalInputResult> HandleCoreAsync(string sessionId, string input, CancellationToken cancellationToken)
    {
        var trimmedInput = (input ?? string.Empty).Trim();
        if (string.Equals(trimmedInput, "/new", StringComparison.OrdinalIgnoreCase))
        {
            var newSessionId = Guid.NewGuid().ToString("N");
            logger.LogInformation("Sessão reiniciada SessionIdAnterior={PreviousSessionId} SessionIdNova={NewSessionId}", sessionId, newSessionId);
            return new MultimodalInputResult
            {
                Message = "Nova sessão iniciada com sucesso.",
                SessionId = newSessionId
            };
        }

        if (trimmedInput.StartsWith("/voice", StringComparison.OrdinalIgnoreCase))
        {
            return HandleVoiceCommand(sessionId, trimmedInput);
        }

        if (trimmedInput.StartsWith("/tts", StringComparison.OrdinalIgnoreCase))
        {
            return HandleTtsCommand(sessionId, trimmedInput);
        }

        if (string.Equals(trimmedInput, "/ptt", StringComparison.OrdinalIgnoreCase))
        {
            return await HandlePushToTalkAsync(sessionId, cancellationToken);
        }

        var inputResult = await inputHandlingService.HandleAsync(sessionId, trimmedInput, cancellationToken);
        var outputMessage = inputResult.Message;

        if (voiceModeState.IsVoiceModeEnabled && voiceModeState.IsTtsEnabled)
        {
            logger.LogInformation("AudioTtsInicio");
            try
            {
                await textToSpeechService.SynthesizeAsync(outputMessage, cancellationToken);
                logger.LogInformation("AudioTtsFim");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AudioTtsFalha");
                voiceModeState.SetVoiceMode(false);
                outputMessage = $"{outputMessage}{Environment.NewLine}[Aviso] Falha na síntese de voz. Retornando para modo texto.";
            }
        }

        return new MultimodalInputResult
        {
            Message = outputMessage,
            SessionId = sessionId,
            Decision = inputResult.Decision
        };
    }

    private MultimodalInputResult HandleVoiceCommand(string sessionId, string input)
    {
        var segments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 1)
        {
            voiceModeState.ToggleVoiceMode();
        }
        else if (segments.Length == 2 && segments[1].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            voiceModeState.SetVoiceMode(true);
        }
        else if (segments.Length == 2 && segments[1].Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            voiceModeState.SetVoiceMode(false);
        }
        else
        {
            return new MultimodalInputResult
            {
                Message = "Uso: /voice [on|off]",
                SessionId = sessionId
            };
        }

        logger.LogInformation("ModoVozAlterado Enabled={VoiceModeEnabled}", voiceModeState.IsVoiceModeEnabled);
        return new MultimodalInputResult
        {
            Message = $"Modo voz {(voiceModeState.IsVoiceModeEnabled ? "ativado" : "desativado")}.",
            SessionId = sessionId
        };
    }

    private MultimodalInputResult HandleTtsCommand(string sessionId, string input)
    {
        var segments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            return new MultimodalInputResult
            {
                Message = "Uso: /tts <on|off>",
                SessionId = sessionId
            };
        }

        if (segments[1].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            voiceModeState.SetTts(true);
        }
        else if (segments[1].Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            voiceModeState.SetTts(false);
        }
        else
        {
            return new MultimodalInputResult
            {
                Message = "Uso: /tts <on|off>",
                SessionId = sessionId
            };
        }

        logger.LogInformation("ModoTtsAlterado Enabled={TtsEnabled}", voiceModeState.IsTtsEnabled);
        return new MultimodalInputResult
        {
            Message = $"TTS {(voiceModeState.IsTtsEnabled ? "ativado" : "desativado")}.",
            SessionId = sessionId
        };
    }

    private async Task<MultimodalInputResult> HandlePushToTalkAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (!voiceModeState.IsVoiceModeEnabled)
        {
            return new MultimodalInputResult
            {
                Message = "Modo voz está desativado. Use /voice on para habilitar.",
                SessionId = sessionId
            };
        }

        logger.LogInformation("AudioCapturaInicio");
        try
        {
            var transcribedText = await speechToTextService.CaptureAndTranscribeAsync(cancellationToken);
            logger.LogInformation("AudioCapturaFim");
            logger.LogInformation("AudioTranscricaoConcluida Length={TranscriptionLength}", transcribedText.Length);

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                return new MultimodalInputResult
                {
                    Message = "Não foi possível entender a fala. Tente novamente.",
                    SessionId = sessionId
                };
            }

            return await HandleCoreAsync(sessionId, transcribedText, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeoutException)
        {
            logger.LogError(ex, "AudioCapturaFalha");
            voiceModeState.SetVoiceMode(false);
            return new MultimodalInputResult
            {
                Message = "Falha ao capturar/transcrever áudio. Retornando para modo texto.",
                SessionId = sessionId
            };
        }
    }
}
