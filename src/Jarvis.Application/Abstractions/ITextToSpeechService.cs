namespace Jarvis.Application.Abstractions;

public interface ITextToSpeechService
{
    Task SynthesizeAsync(string text, CancellationToken cancellationToken = default);
}
