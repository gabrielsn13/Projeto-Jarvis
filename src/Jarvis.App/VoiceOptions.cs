namespace Jarvis.App;

public sealed class VoiceOptions
{
    public const string SectionName = "Voice";
    public bool Enabled { get; init; }
    public bool TtsEnabled { get; init; }
    public int CaptureTimeoutSeconds { get; init; } = 10;
}
