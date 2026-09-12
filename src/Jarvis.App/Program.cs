using Jarvis.Application;
using Jarvis.Application.Abstractions;
using Jarvis.App;
using Jarvis.App.Services;
using Jarvis.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Services existentes
builder.Services
    .AddJarvisApplication()
    .AddJarvisInfrastructure(builder.Configuration);

builder.Services.Configure<VoiceOptions>(builder.Configuration.GetSection(VoiceOptions.SectionName));

// Providers
var voiceSection = builder.Configuration.GetSection(VoiceOptions.SectionName);
var sttProvider = voiceSection["SttProvider"] ?? "Console";
var ttsProvider = voiceSection["TtsProvider"] ?? "PowerShell";

if (string.Equals(sttProvider, "Whisper", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<ISpeechToTextService, WhisperSpeechToTextService>();
else
    builder.Services.AddSingleton<ISpeechToTextService, ConsolePushToTalkSpeechToTextService>();

builder.Services.AddSingleton<PowerShellTextToSpeechService>();

if (string.Equals(ttsProvider, "Edge", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<ITextToSpeechService, EdgeTextToSpeechService>();
else
    builder.Services.AddSingleton<ITextToSpeechService>(sp => sp.GetRequiredService<PowerShellTextToSpeechService>());

builder.Services.AddSingleton<IVoiceRuntimeSettings, Jarvis.Application.Services.VoiceRuntimeSettings>();

builder.Services.AddSingleton<IVoiceModeState>(sp =>
{
    var options = sp.GetRequiredService<IOptions<VoiceOptions>>().Value;
    return new Jarvis.Application.Services.VoiceModeState(options.Enabled, options.TtsEnabled);
});

// CORS para Unity local
builder.Services.AddCors(o =>
{
    o.AddPolicy("unity-local", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

var app = builder.Build();
app.UseCors("unity-local");

// init repositório
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IChatHistoryRepository>();
    await repo.InitializeAsync();
}

app.MapGet("/health", () => Results.Ok(new { ok = true, service = "jarvis", utc = DateTime.UtcNow }));

app.MapPost("/chat", async (
    ChatRequest req,
    IMultimodalInputService multimodal,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest(new { error = "message is required" });

    var sessionId = string.IsNullOrWhiteSpace(req.SessionId) ? "default" : req.SessionId!;
    var result = await multimodal.HandleAsync(sessionId, req.Message, ct);

    return Results.Ok(new ChatResponse(
        result.SessionId,
        result.Message
    ));
});

app.MapPost("/tts", async (
    TtsRequest req,
    ITextToSpeechService tts,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest(new { error = "text is required" });

    await tts.SynthesizeAsync(req.Text, ct);
    return Results.Ok(new { ok = true });
});

app.Run("http://127.0.0.1:5077");

public sealed record ChatRequest(string? SessionId, string Message);
public sealed record ChatResponse(string SessionId, string Message);
public sealed record TtsRequest(string Text);