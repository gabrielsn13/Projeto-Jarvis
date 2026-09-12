using Jarvis.Application.Abstractions;

namespace Jarvis.Application.Services;

public sealed class VoiceModeState(bool voiceModeEnabled, bool ttsEnabled) : IVoiceModeState
{
    public bool IsVoiceModeEnabled { get; private set; } = voiceModeEnabled;
    public bool IsTtsEnabled { get; private set; } = ttsEnabled;

    public void SetVoiceMode(bool enabled)
    {
        IsVoiceModeEnabled = enabled;
    }

    public void ToggleVoiceMode()
    {
        IsVoiceModeEnabled = !IsVoiceModeEnabled;
    }

    public void SetTts(bool enabled)
    {
        IsTtsEnabled = enabled;
    }
}
