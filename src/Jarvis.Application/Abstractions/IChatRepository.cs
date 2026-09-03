using Jarvis.Domain.Entities;

namespace Jarvis.Application.Abstractions;

public interface IChatRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(int limit, CancellationToken cancellationToken = default);
}
