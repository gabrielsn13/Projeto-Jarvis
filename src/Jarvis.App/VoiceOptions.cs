namespace Jarvis.App;

public sealed class VoiceOptions
{
    public const string SectionName = "Voice";

    public bool Enabled { get; set; } = false;
    public bool TtsEnabled { get; set; } = true;

    public int CaptureTimeoutSeconds { get; set; } = 8;
    public int TranscriptionTimeoutSeconds { get; set; } = 30;

    public string Language { get; set; } = "pt";
    public string SttProvider { get; set; } = "Console"; // Console | Whisper

    public string? PythonExecutablePath { get; set; } = "python";
    public string? WhisperScriptPath { get; set; } = "scripts/stt_whisper.py";
    public string? WhisperModel { get; set; } = "base";
}