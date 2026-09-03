namespace Jarvis.Domain.Entities;

public sealed class ChatMessage
{
    public required string Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
