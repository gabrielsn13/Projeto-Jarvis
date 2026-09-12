using System.Diagnostics;
using Jarvis.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;

namespace Jarvis.App.Services;

public sealed class WhisperSpeechToTextService(
    IOptions<VoiceOptions> voiceOptions,
    ILogger<WhisperSpeechToTextService> logger) : ISpeechToTextService
{
    public async Task<string> CaptureAndTranscribeAsync(CancellationToken cancellationToken = default)
    {
        var opts = voiceOptions.Value;

        var captureSeconds = Math.Max(1, opts.CaptureTimeoutSeconds);
        var transcriptionSeconds = Math.Max(5, opts.TranscriptionTimeoutSeconds);

        var tempWavPath = Path.Combine(Path.GetTempPath(), $"jarvis-stt-{Guid.NewGuid():N}.wav");

        try
        {
            logger.LogInformation(
                "AudioCapturaInicio provider=WhisperLocal captureTimeout={CaptureSeconds}s transcriptionTimeout={TranscriptionSeconds}s model={Model} lang={Lang}",
                captureSeconds, transcriptionSeconds, opts.WhisperModel ?? "small", opts.Language ?? "pt");

            // 1) Captura (timeout próprio)
            using (var captureCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                captureCts.CancelAfter(TimeSpan.FromSeconds(captureSeconds));

                try
                {
                    await CaptureWavFromMicrophoneAsync(
                        outputPath: tempWavPath,
                        maxDuration: TimeSpan.FromSeconds(captureSeconds),
                        cancellationToken: captureCts.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("AudioCapturaTimeout apos {CaptureSeconds}s", captureSeconds);
                    throw new TimeoutException($"Tempo limite excedido na captura de áudio ({captureSeconds}s).");
                }
            }

            logger.LogInformation("AudioCapturaFim path={TempWavPath}", tempWavPath);

            // 2) Transcrição (timeout próprio)
            string transcription;
            using (var transcriptionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                transcriptionCts.CancelAfter(TimeSpan.FromSeconds(transcriptionSeconds));

                try
                {
                    transcription = await RunWhisperPythonAsync(
                        audioPath: tempWavPath,
                        options: opts,
                        cancellationToken: transcriptionCts.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("AudioTranscricaoTimeout apos {TranscriptionSeconds}s", transcriptionSeconds);
                    throw new TimeoutException($"Tempo limite excedido na transcrição de áudio ({transcriptionSeconds}s).");
                }
            }

            if (string.IsNullOrWhiteSpace(transcription))
            {
                throw new InvalidOperationException("Nenhuma fala/texto capturado.");
            }

            logger.LogInformation("AudioTranscricaoSucesso chars={Length}", transcription.Length);
            return transcription.Trim();
        }
        catch (TimeoutException)
        {
            // já logado acima com contexto específico
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AudioCapturaFalha");
            throw;
        }
        finally
        {
            TryDeleteFile(tempWavPath);
        }
    }

    private async Task CaptureWavFromMicrophoneAsync(
        string outputPath,
        TimeSpan maxDuration,
        CancellationToken cancellationToken)
    {
        var tcsStopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using var waveIn = new WaveInEvent
        {
            DeviceNumber = 0, // microfone padrão
            WaveFormat = new WaveFormat(16000, 16, 1), // 16kHz mono
            BufferMilliseconds = 100
        };

        using var writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);

        waveIn.DataAvailable += (_, e) =>
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            writer.Flush();
        };

        waveIn.RecordingStopped += (_, e) =>
        {
            if (e.Exception is not null)
                tcsStopped.TrySetException(e.Exception);
            else
                tcsStopped.TrySetResult();
        };

        waveIn.StartRecording();

        try
        {
            // Garante parada automática por duração máxima ou cancelamento externo
            using var durationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            durationCts.CancelAfter(maxDuration);

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, durationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // esperado ao atingir duração máxima ou cancelamento
            }
        }
        finally
        {
            waveIn.StopRecording();
            await tcsStopped.Task.WaitAsync(CancellationToken.None);
        }
    }

    private async Task<string> RunWhisperPythonAsync(
        string audioPath,
        VoiceOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.WhisperScriptPath))
            throw new InvalidOperationException("WhisperScriptPath não configurado.");

        var pythonExe = string.IsNullOrWhiteSpace(options.PythonExecutablePath)
            ? "python"
            : options.PythonExecutablePath!;

        var scriptPath = ResolveScriptPath(options.WhisperScriptPath!);
        if (!File.Exists(scriptPath))
            throw new InvalidOperationException($"Script Whisper não encontrado em: {scriptPath}");

        var model = string.IsNullOrWhiteSpace(options.WhisperModel) ? "small" : options.WhisperModel!;
        var language = string.IsNullOrWhiteSpace(options.Language) ? "pt" : options.Language!;

        // Aspas para suportar espaços no path
        var args = $"\"{scriptPath}\" \"{audioPath}\" --model \"{model}\" --language \"{language}\"";

        logger.LogInformation(
            "AudioTranscricaoInicio engine=faster-whisper model={Model} lang={Language} python={PythonExe} script={ScriptPath}",
            model, language, pythonExe, scriptPath);

        var psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Não foi possível iniciar o processo Python ('{pythonExe}'). Verifique PythonExecutablePath/PATH.",
                ex);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = (await stdoutTask).Trim();
        var stderr = (await stderrTask).Trim();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Falha no Whisper (exit={process.ExitCode}). Detalhes: {stderr}");
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            // libs podem escrever warnings no stderr mesmo com sucesso
            logger.LogDebug("Whisper stderr: {Stderr}", stderr);
        }

        logger.LogInformation("AudioTranscricaoFim");
        return stdout;
    }

    private static string ResolveScriptPath(string configuredPath)
    {
        // 1) Se já for absoluto, usa direto
        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        // 2) Tenta relativo ao diretório de execução (bin/Debug/net8.0)
        var baseDir = AppContext.BaseDirectory;
        var candidateFromBase = Path.GetFullPath(Path.Combine(baseDir, configuredPath));
        if (File.Exists(candidateFromBase))
            return candidateFromBase;

        // 3) Tenta relativo ao diretório atual do processo
        var cwd = Directory.GetCurrentDirectory();
        var candidateFromCwd = Path.GetFullPath(Path.Combine(cwd, configuredPath));
        return candidateFromCwd;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // no-op
        }
    }
}