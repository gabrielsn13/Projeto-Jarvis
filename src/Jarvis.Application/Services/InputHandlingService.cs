using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Microsoft.Extensions.Logging;

namespace Jarvis.Application.Services;

public sealed class InputHandlingService(
    IIntentRouter intentRouter,
    IChatService chatService,
    ICommandService commandService,
    ILogger<InputHandlingService> logger) : IInputHandlingService
{
    private const string UnknownCommandMessage = "Comando inválido ou bloqueado. Use /cmd <comando>";

    public async Task<InputHandlingResult> HandleAsync(string sessionId, string input, CancellationToken cancellationToken = default)
    {
        var route = intentRouter.Classify(input);

        InputHandlingResult result;
        switch (route.Intent)
        {
            case InputIntent.Command:
            {
                var commandResult = await commandService.ExecuteAsync(route.NormalizedCommand ?? string.Empty, cancellationToken);
                result = new InputHandlingResult
                {
                    Message = BuildCommandMessage(commandResult),
                    Decision = commandResult.Status == CommandExecutionStatus.Success
                        ? InputHandlingDecision.Executed
                        : InputHandlingDecision.Blocked,
                    NormalizedCommand = route.NormalizedCommand
                };
                break;
            }
            case InputIntent.UnknownCommand:
                result = new InputHandlingResult
                {
                    Message = UnknownCommandMessage,
                    Decision = InputHandlingDecision.Blocked,
                    NormalizedCommand = route.NormalizedCommand
                };
                break;
            default:
            {
                var response = await chatService.SendMessageAsync(sessionId, input, cancellationToken);
                result = new InputHandlingResult
                {
                    Message = response.Content,
                    Decision = InputHandlingDecision.Chat,
                    NormalizedCommand = route.NormalizedCommand
                };
                break;
            }
        }

        logger.LogInformation(
            "RoteamentoEntrada Intent={Intent} NormalizedCommand={NormalizedCommand} Decision={Decision}",
            route.Intent,
            route.NormalizedCommand ?? string.Empty,
            result.Decision);

        return result;
    }

    private static string BuildCommandMessage(CommandExecutionResult commandResult)
    {
        if (commandResult.Status == CommandExecutionStatus.Success)
        {
            return commandResult.Message.StartsWith("Comando executado com sucesso.", StringComparison.OrdinalIgnoreCase)
                ? commandResult.Message
                : $"Comando executado com sucesso. {commandResult.Message}";
        }

        if (commandResult.Status == CommandExecutionStatus.Blocked)
        {
            return UnknownCommandMessage;
        }

        return commandResult.Message;
    }
}
