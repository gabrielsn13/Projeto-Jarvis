using System.Diagnostics;
using Jarvis.Application.Abstractions;

namespace Jarvis.App.Services;

public sealed class PowerShellTextToSpeechService : ITextToSpeechService
{
    public async Task SynthesizeAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("TTS indisponível fora do Windows.");
        }

        var escapedText = text.Replace("'", "''", StringComparison.Ordinal);
        var arguments = "-NoProfile -Command \"Add-Type -AssemblyName System.Speech; " +
                        $"$s = New-Object System.Speech.Synthesis.SpeechSynthesizer; $s.Speak('{escapedText}')\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Falha ao iniciar serviço de síntese.");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Falha na síntese de voz.");
        }
    }
}
