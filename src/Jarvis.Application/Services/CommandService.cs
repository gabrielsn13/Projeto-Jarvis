using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Microsoft.Extensions.Logging;

namespace Jarvis.Application.Services;

public sealed class CommandService(
    ICommandCatalog commandCatalog,
    ICommandExecutor commandExecutor,
    ILogger<CommandService> logger) : ICommandService
{
    public async Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default)
    {
        var normalizedCommand = (commandId ?? string.Empty).Trim().ToLowerInvariant();

        if (!commandCatalog.IsAllowed(normalizedCommand))
        {
            var blockedResult = new CommandExecutionResult
            {
                CommandId = normalizedCommand,
                Status = CommandExecutionStatus.Blocked,
                Message = "Comando bloqueado: comando não permitido nesta fase.",
                FailureReason = "Comando fora da allowlist."
            };

            LogAudit(normalizedCommand, "blocked", blockedResult);
            return blockedResult;
        }

        try
        {
            var result = await commandExecutor.ExecuteAsync(normalizedCommand, cancellationToken);
            LogAudit(normalizedCommand, "allowed", result);
            return result;
        }
        catch (Exception ex)
        {
            var failedResult = new CommandExecutionResult
            {
                CommandId = normalizedCommand,
                Status = CommandExecutionStatus.Failure,
                Message = "Não foi possível executar o comando no momento.",
                FailureReason = ex.Message
            };

            logger.LogError(ex, "Falha inesperada ao executar comando permitido {CommandId}.", normalizedCommand);
            LogAudit(normalizedCommand, "allowed", failedResult);
            return failedResult;
        }
    }

    private void LogAudit(string commandId, string decision, CommandExecutionResult result)
    {
        logger.LogInformation(
            "AuditoriaComando RequestedCommand={RequestedCommand} Decision={Decision} Result={Result} Reason={Reason}",
            commandId,
            decision,
            result.Status,
            result.FailureReason ?? string.Empty);
    }
}
