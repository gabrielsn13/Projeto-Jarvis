using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);
}
