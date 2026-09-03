using Jarvis.Application;
using Jarvis.Application.Abstractions;
using Jarvis.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services
    .AddJarvisApplication()
    .AddJarvisInfrastructure(builder.Configuration);

using var host = builder.Build();

using var scope = host.Services.CreateScope();
var provider = scope.ServiceProvider;
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Jarvis.App");
var repository = provider.GetRequiredService<Jarvis.Application.Abstractions.IChatRepository>();
var chatService = provider.GetRequiredService<IChatService>();

await repository.InitializeAsync();

logger.LogInformation("JARVIS iniciado. Digite sua mensagem (ou 'sair').");

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

    var response = await chatService.SendMessageAsync(input);
    Console.WriteLine($"Jarvis: {response.Content}");
}

logger.LogInformation("JARVIS finalizado.");
