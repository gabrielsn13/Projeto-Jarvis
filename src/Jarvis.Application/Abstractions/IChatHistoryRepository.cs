using Jarvis.Domain.Entities;

namespace Jarvis.Application.Abstractions;

public interface IChatHistoryRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task AddMessageAsync(string sessionId, ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(string sessionId, int limit, CancellationToken cancellationToken = default);
}
