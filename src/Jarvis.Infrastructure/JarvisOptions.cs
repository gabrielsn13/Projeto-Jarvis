namespace Jarvis.Infrastructure;

public sealed class JarvisOptions
{
    public const string SectionName = "Jarvis";

    public string Model { get; init; } = "llama3.2";
    public string OllamaBaseUrl { get; init; } = "http://localhost:11434";
    public string DatabasePath { get; init; } = "jarvis.db";
}
