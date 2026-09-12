using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default);
}
