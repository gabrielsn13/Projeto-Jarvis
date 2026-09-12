namespace Jarvis.Application.Models;

public sealed class CommandExecutionResult
{
    public required string CommandId { get; init; }
    public required CommandExecutionStatus Status { get; init; }
    public required string Message { get; init; }
    public string? FailureReason { get; init; }
}
