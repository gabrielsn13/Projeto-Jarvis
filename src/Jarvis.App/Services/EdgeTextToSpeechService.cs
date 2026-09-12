using System.Diagnostics;
using Jarvis.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.App.Services;

public sealed class EdgeTextToSpeechService(
    IOptions<VoiceOptions> optionsAccessor,
    ILogger<EdgeTextToSpeechService> logger) : ITextToSpeechService
{
    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var opts = optionsAccessor.Value;
        if (string.IsNullOrWhiteSpace(text))
            return;

        var python = string.IsNullOrWhiteSpace(opts.PythonExecutablePath) ? "python" : opts.PythonExecutablePath!;
        var script = ResolvePath(opts.EdgeTtsScriptPath ?? "scripts/tts_edge.py");

        if (!File.Exists(script))
            throw new InvalidOperationException($"Script TTS não encontrado: {script}");

        var voice = string.IsNullOrWhiteSpace(opts.EdgeVoice) ? "pt-BR-AntonioNeural" : opts.EdgeVoice!;
        var rate = string.IsNullOrWhiteSpace(opts.EdgeRate) ? "+0%" : opts.EdgeRate!;
        var volume = string.IsNullOrWhiteSpace(opts.EdgeVolume) ? "+0%" : opts.EdgeVolume!;
        var pitch = string.IsNullOrWhiteSpace(opts.EdgePitch) ? "+0Hz" : opts.EdgePitch!;

        var args =
            $"\"{script}\" --text \"{EscapeArg(text)}\" --voice \"{voice}\" --rate \"{rate}\" --volume \"{volume}\" --pitch \"{pitch}\"";

        var psi = new ProcessStartInfo
        {
            FileName = python,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = new Process { StartInfo = psi };
        p.Start();

        var stdoutTask = p.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = p.StandardError.ReadToEndAsync(cancellationToken);

        await p.WaitForExitAsync(cancellationToken);

        var stdout = (await stdoutTask).Trim();
        var stderr = (await stderrTask).Trim();

        if (p.ExitCode != 0)
            throw new InvalidOperationException($"Edge TTS falhou (exit={p.ExitCode}): {stderr}");

        if (string.IsNullOrWhiteSpace(stdout) || !File.Exists(stdout))
            throw new InvalidOperationException("Edge TTS não retornou arquivo de áudio válido.");

        var audioPath = stdout;
        logger.LogInformation("TTS gerado em {Path}", audioPath);

        await PlayAndDeleteAsync(audioPath, cancellationToken);
    }

    public Task SynthesizeAsync(string text, CancellationToken cancellationToken = default)
        => SpeakAsync(text, cancellationToken);

    private static async Task PlayAndDeleteAsync(string audioPath, CancellationToken ct)
    {
        Process? player = null;
        try
        {
            player = Process.Start(new ProcessStartInfo
            {
                FileName = audioPath,
                UseShellExecute = true
            });

            if (player is not null)
                await player.WaitForExitAsync(ct);
            else
                await Task.Delay(3000, ct);
        }
        finally
        {
            for (var i = 0; i < 8; i++)
            {
                try
                {
                    if (File.Exists(audioPath))
                        File.Delete(audioPath);
                    break;
                }
                catch (IOException) when (i < 7)
                {
                    await Task.Delay(250, ct);
                }
                catch (UnauthorizedAccessException) when (i < 7)
                {
                    await Task.Delay(250, ct);
                }
            }
        }
    }

    private static string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath)) return configuredPath;

        var baseDir = AppContext.BaseDirectory;
        var p1 = Path.GetFullPath(Path.Combine(baseDir, configuredPath));
        if (File.Exists(p1)) return p1;

        var cwd = Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(cwd, configuredPath));
    }

    private static string EscapeArg(string value)
        => value.Replace("\"", "\\\"");
}