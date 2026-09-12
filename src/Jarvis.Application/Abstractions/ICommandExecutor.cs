using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface ICommandExecutor
{
    Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default);
}
