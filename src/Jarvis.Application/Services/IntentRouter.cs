using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;

namespace Jarvis.Application.Services;

public sealed class IntentRouter(ICommandCatalog commandCatalog) : IIntentRouter
{
    private const string CommandPrefix = "/cmd";

    public IntentRoutingResult Classify(string input)
    {
        var trimmedInput = (input ?? string.Empty).Trim();
        if (!trimmedInput.StartsWith(CommandPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.Chat
            };
        }

        if (trimmedInput.Length > CommandPrefix.Length && !char.IsWhiteSpace(trimmedInput[CommandPrefix.Length]))
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.Chat
            };
        }

        if (trimmedInput.Length == CommandPrefix.Length)
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.UnknownCommand
            };
        }

        var commandSegment = trimmedInput[CommandPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(commandSegment))
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.UnknownCommand
            };
        }

        var commandParts = commandSegment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (commandParts.Length != 1)
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.UnknownCommand,
                NormalizedCommand = commandSegment.ToLowerInvariant()
            };
        }

        var normalizedCommand = commandParts[0].Trim().ToLowerInvariant();
        if (!commandCatalog.IsAllowed(normalizedCommand))
        {
            return new IntentRoutingResult
            {
                Intent = InputIntent.UnknownCommand,
                NormalizedCommand = normalizedCommand
            };
        }

        return new IntentRoutingResult
        {
            Intent = InputIntent.Command,
            NormalizedCommand = normalizedCommand
        };
    }
}
