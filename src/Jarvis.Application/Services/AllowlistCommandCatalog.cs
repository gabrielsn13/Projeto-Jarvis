using Jarvis.Application.Abstractions;

namespace Jarvis.Application.Services;

public sealed class AllowlistCommandCatalog : ICommandCatalog
{
    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "abrir_notepad",
        "abrir_calculadora",
        "mostrar_data_hora"
    };

    public bool IsAllowed(string commandId)
    {
        return !string.IsNullOrWhiteSpace(commandId) && AllowedCommands.Contains(commandId.Trim());
    }
}
