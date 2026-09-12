namespace Jarvis.Application.Abstractions;

public interface IVoiceModeState
{
    bool IsVoiceModeEnabled { get; }
    bool IsTtsEnabled { get; }
    void SetVoiceMode(bool enabled);
    void ToggleVoiceMode();
    void SetTts(bool enabled);
}
