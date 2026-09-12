using Jarvis.Application.Abstractions;
using Jarvis.Infrastructure.Llm;
using Jarvis.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddJarvisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<SqliteOptions>(configuration.GetSection(SqliteOptions.SectionName));

        services.AddHttpClient<ILLMProvider, OllamaProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IChatHistoryRepository, SqliteChatHistoryRepository>();

        return services;
    }
}
