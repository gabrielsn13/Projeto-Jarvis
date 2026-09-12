namespace Jarvis.Application.Abstractions;

public interface ICommandCatalog
{
    bool IsAllowed(string commandId);
}
