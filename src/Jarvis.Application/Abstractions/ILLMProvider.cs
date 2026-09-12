using Jarvis.Domain.Entities;

namespace Jarvis.Application.Abstractions;

public interface ILLMProvider
{
    Task<string> GenerateResponseAsync(string prompt, IReadOnlyList<ChatMessage> context, CancellationToken cancellationToken = default);
}
