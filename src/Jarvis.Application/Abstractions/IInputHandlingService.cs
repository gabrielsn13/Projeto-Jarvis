using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface IInputHandlingService
{
    Task<InputHandlingResult> HandleAsync(string sessionId, string input, CancellationToken cancellationToken = default);
}
