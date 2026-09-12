using Jarvis.Application.Abstractions;

namespace Jarvis.Application.Services;

public sealed class VoiceRuntimeSettings : IVoiceRuntimeSettings
{
    // Você pode expandir essa lista depois
    private static readonly string[] Voices =
    [
        "pt-BR-AntonioNeural",
        "pt-BR-FranciscaNeural",
        "en-US-GuyNeural",
        "en-US-JennyNeural"
    ];

    public string EdgeVoice { get; set; } = "pt-BR-AntonioNeural";
    public string EdgeRate { get; set; } = "+0%";
    public IReadOnlyList<string> AvailableVoices => Voices;
}