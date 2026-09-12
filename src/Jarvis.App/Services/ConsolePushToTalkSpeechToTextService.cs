using Jarvis.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.App.Services;

public sealed class ConsolePushToTalkSpeechToTextService(
    IOptions<VoiceOptions> voiceOptions,
    ILogger<ConsolePushToTalkSpeechToTextService> logger) : ISpeechToTextService
{
    public async Task<string> CaptureAndTranscribeAsync(CancellationToken cancellationToken = default)
    {
        Console.Write("🎙️  Push-to-talk (digite a transcrição): ");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, voiceOptions.Value.CaptureTimeoutSeconds)));

        try
        {
            var transcription = await Task.Run(Console.ReadLine, timeoutCts.Token);
            if (transcription is null)
            {
                throw new InvalidOperationException("Microfone indisponível no momento.");
            }

            return transcription.Trim();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timeout na captura de áudio.");
            throw new TimeoutException("Tempo limite excedido para captura de áudio.");
        }
    }
}
