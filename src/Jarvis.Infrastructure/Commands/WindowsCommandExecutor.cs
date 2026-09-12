using System.Diagnostics;
using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;

namespace Jarvis.Infrastructure.Commands;

public sealed class WindowsCommandExecutor : ICommandExecutor
{
    public Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default)
    {
        return commandId switch
        {
            "abrir_notepad" => ExecuteProcessCommandAsync(commandId, "notepad"),
            "abrir_calculadora" => ExecuteProcessCommandAsync(commandId, "calc"),
            "mostrar_data_hora" => Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Success,
                Message = $"Comando executado com sucesso. Data e hora locais: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"
            }),
            _ => Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Blocked,
                Message = "Comando bloqueado: comando não permitido nesta fase.",
                FailureReason = "Comando fora da allowlist."
            })
        };
    }

    private static Task<CommandExecutionResult> ExecuteProcessCommandAsync(string commandId, string fileName)
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Failure,
                Message = "Esse comando está disponível apenas no Windows.",
                FailureReason = "Sistema operacional incompatível."
            });
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return Task.FromResult(new CommandExecutionResult
                {
                    CommandId = commandId,
                    Status = CommandExecutionStatus.Failure,
                    Message = "Não foi possível iniciar o comando local.",
                    FailureReason = "Process.Start retornou nulo."
                });
            }

            return Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Success,
                Message = "Comando executado com sucesso."
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Failure,
                Message = "Não foi possível executar o comando no momento.",
                FailureReason = ex.Message
            });
        }
    }
}
