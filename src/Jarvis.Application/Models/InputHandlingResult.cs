namespace Jarvis.Application.Models;

public sealed class InputHandlingResult
{
    public required string Message { get; init; }
    public required InputHandlingDecision Decision { get; init; }
    public string? NormalizedCommand { get; init; }
}
