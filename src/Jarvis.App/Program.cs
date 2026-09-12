using Jarvis.Application;
using Jarvis.Application.Abstractions;
using Jarvis.App;
using Jarvis.App.Services;
using Jarvis.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
});

builder.Services
    .AddJarvisApplication()
    .AddJarvisInfrastructure(builder.Configuration);

builder.Services.Configure<VoiceOptions>(builder.Configuration.GetSection(VoiceOptions.SectionName));

// Seleção de STT por configuração
var voiceSection = builder.Configuration.GetSection(VoiceOptions.SectionName);
var sttProvider = voiceSection["SttProvider"] ?? "Console";

if (string.Equals(sttProvider, "Whisper", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ISpeechToTextService, WhisperSpeechToTextService>();
}
else
{
    builder.Services.AddSingleton<ISpeechToTextService, ConsolePushToTalkSpeechToTextService>();
}

builder.Services.AddSingleton<ITextToSpeechService, PowerShellTextToSpeechService>();
builder.Services.AddSingleton<IVoiceModeState>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<VoiceOptions>>().Value;
    return new Jarvis.Application.Services.VoiceModeState(options.Enabled, options.TtsEnabled);
});

using var host = builder.Build();

using var scope = host.Services.CreateScope();
var provider = scope.ServiceProvider;
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Jarvis.App");

// Log útil para validar em runtime
logger.LogInformation("STT provider selecionado: {Provider}", sttProvider);

var repository = provider.GetRequiredService<Jarvis.Application.Abstractions.IChatHistoryRepository>();
var multimodalInputService = provider.GetRequiredService<IMultimodalInputService>();
var sessionId = "default";

await repository.InitializeAsync();

logger.LogInformation("JARVIS iniciado. Digite sua mensagem (ou 'sair'). Comandos: /voice [on|off], /tts <on|off>, /ptt, /new");

while (true)
{
    Console.Write("Você: ");
    var input = Console.ReadLine();

    if (string.Equals(input, "sair", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    try
    {
        var result = await multimodalInputService.HandleAsync(sessionId, input);
        sessionId = result.SessionId;
        Console.WriteLine($"Jarvis: {result.Message}");
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Falha no fluxo de chat.");
        Console.WriteLine($"Jarvis: {ex.Message}");
    }
}

logger.LogInformation("JARVIS finalizado.");