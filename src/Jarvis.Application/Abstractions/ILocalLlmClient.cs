using Jarvis.Domain.Entities;

namespace Jarvis.Application.Abstractions;

public interface ILocalLlmClient
{
    Task<string> GenerateReplyAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
}
