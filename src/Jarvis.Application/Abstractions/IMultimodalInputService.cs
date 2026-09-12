using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface IMultimodalInputService
{
    Task<MultimodalInputResult> HandleAsync(string sessionId, string input, CancellationToken cancellationToken = default);
}
