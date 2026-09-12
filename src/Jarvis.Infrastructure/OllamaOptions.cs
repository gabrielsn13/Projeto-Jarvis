namespace Jarvis.Infrastructure;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "llama3.2";
    public int TimeoutSeconds { get; init; } = 60;
}
