using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface ICommandService
{
    Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default);
}
