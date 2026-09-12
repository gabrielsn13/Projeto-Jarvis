namespace Jarvis.Application.Abstractions;

public interface ISpeechToTextService
{
    Task<string> CaptureAndTranscribeAsync(CancellationToken cancellationToken = default);
}
