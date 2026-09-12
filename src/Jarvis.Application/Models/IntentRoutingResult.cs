namespace Jarvis.Application.Models;

public sealed class IntentRoutingResult
{
    public required InputIntent Intent { get; init; }
    public string? NormalizedCommand { get; init; }
}
