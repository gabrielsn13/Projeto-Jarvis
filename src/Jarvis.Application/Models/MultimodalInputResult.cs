namespace Jarvis.Application.Models;

public sealed class MultimodalInputResult
{
    public required string Message { get; init; }
    public required string SessionId { get; init; }
    public InputHandlingDecision? Decision { get; init; }
}
