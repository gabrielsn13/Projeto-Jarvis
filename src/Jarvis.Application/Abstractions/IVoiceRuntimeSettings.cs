namespace Jarvis.Application.Abstractions;

public interface IVoiceRuntimeSettings
{
    string EdgeVoice { get; set; }
    string EdgeRate { get; set; }
    IReadOnlyList<string> AvailableVoices { get; }
}