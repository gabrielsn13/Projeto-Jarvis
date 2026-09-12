using Jarvis.Application.Models;

namespace Jarvis.Application.Abstractions;

public interface IIntentRouter
{
    IntentRoutingResult Classify(string input);
}
