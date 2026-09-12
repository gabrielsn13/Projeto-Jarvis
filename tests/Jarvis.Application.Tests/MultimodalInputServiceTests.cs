using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Jarvis.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jarvis.Application.Tests;

public sealed class MultimodalInputServiceTests
{
    [Fact]
    public async Task HandleAsync_ShouldToggleVoiceMode_WithVoiceCommand()
    {
        var state = new VoiceModeState(false, true);
        var service = CreateService(state);

        var result = await service.HandleAsync("default", "/voice");

        Assert.True(state.IsVoiceModeEnabled);
        Assert.Equal("Modo voz ativado.", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldFallbackToText_WhenSttFails()
    {
        var state = new VoiceModeState(true, true);
        var service = CreateService(state, speechToTextService: new FailingSpeechToTextService(new TimeoutException("timeout")));

        var result = await service.HandleAsync("default", "/ptt");

        Assert.False(state.IsVoiceModeEnabled);
        Assert.Equal("Falha ao capturar/transcrever áudio. Retornando para modo texto.", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldFallbackToText_WhenTtsFails()
    {
        var state = new VoiceModeState(true, true);
        var service = CreateService(
            state,
            inputHandlingService: new FakeInputHandlingService("resposta em texto"),
            textToSpeechService: new FailingTextToSpeechService());

        var result = await service.HandleAsync("default", "Olá");

        Assert.False(state.IsVoiceModeEnabled);
        Assert.Contains("resposta em texto", result.Message);
        Assert.Contains("Falha na síntese de voz", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallInputHandling_WhenInputIsNewSessionCommand()
    {
        var inputHandling = new FakeInputHandlingService("não deve chamar");
        var service = CreateService(new VoiceModeState(false, true), inputHandlingService: inputHandling);

        var result = await service.HandleAsync("session-a", "/new");

        Assert.Equal(0, inputHandling.CallCount);
        Assert.NotEqual("session-a", result.SessionId);
        Assert.Equal("Nova sessão iniciada com sucesso.", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldKeepSessionAndRouteTranscribedCommand()
    {
        var inputHandling = new FakeInputHandlingService("ok");
        var service = CreateService(
            new VoiceModeState(true, false),
            inputHandlingService: inputHandling,
            speechToTextService: new SuccessfulSpeechToTextService("/cmd abrir_notepad"));

        var result = await service.HandleAsync("session-1", "/ptt");

        Assert.Equal("session-1", result.SessionId);
        Assert.Equal(1, inputHandling.CallCount);
        Assert.Equal("/cmd abrir_notepad", inputHandling.LastInput);
        Assert.Equal("ok", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldResetSession_WhenSpeechTranscribesNewSessionCommand()
    {
        var inputHandling = new FakeInputHandlingService("não deve chamar");
        var service = CreateService(
            new VoiceModeState(true, false),
            inputHandlingService: inputHandling,
            speechToTextService: new SuccessfulSpeechToTextService("/new"));

        var result = await service.HandleAsync("session-1", "/ptt");

        Assert.Equal(0, inputHandling.CallCount);
        Assert.NotEqual("session-1", result.SessionId);
        Assert.Equal("Nova sessão iniciada com sucesso.", result.Message);
    }

    private static MultimodalInputService CreateService(
        VoiceModeState voiceModeState,
        IInputHandlingService? inputHandlingService = null,
        ISpeechToTextService? speechToTextService = null,
        ITextToSpeechService? textToSpeechService = null)
    {
        return new MultimodalInputService(
            inputHandlingService ?? new FakeInputHandlingService("resposta padrão"),
            speechToTextService ?? new SuccessfulSpeechToTextService("olá"),
            textToSpeechService ?? new NoOpTextToSpeechService(),
            voiceModeState,
            NullLogger<MultimodalInputService>.Instance);
    }

    private sealed class FakeInputHandlingService(string response) : IInputHandlingService
    {
        public int CallCount { get; private set; }
        public string? LastInput { get; private set; }

        public Task<InputHandlingResult> HandleAsync(string sessionId, string input, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastInput = input;
            return Task.FromResult(new InputHandlingResult
            {
                Message = response,
                Decision = InputHandlingDecision.Chat
            });
        }
    }

    private sealed class SuccessfulSpeechToTextService(string transcription) : ISpeechToTextService
    {
        public Task<string> CaptureAndTranscribeAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(transcription);
        }
    }

    private sealed class FailingSpeechToTextService(Exception exception) : ISpeechToTextService
    {
        public Task<string> CaptureAndTranscribeAsync(CancellationToken cancellationToken = default)
        {
            throw exception;
        }
    }

    private sealed class NoOpTextToSpeechService : ITextToSpeechService
    {
        public Task SynthesizeAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FailingTextToSpeechService : ITextToSpeechService
    {
        public Task SynthesizeAsync(string text, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("tts indisponível");
        }
    }
}
