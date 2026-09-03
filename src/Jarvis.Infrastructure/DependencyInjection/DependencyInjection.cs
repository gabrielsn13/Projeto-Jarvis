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
        services.Configure<JarvisOptions>(configuration.GetSection(JarvisOptions.SectionName));

        services.AddHttpClient<ILocalLlmClient, OllamaLocalLlmClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JarvisOptions>>().Value;
            client.BaseAddress = new Uri(options.OllamaBaseUrl);
        });

        services.AddScoped<IChatRepository, SqliteChatRepository>();

        return services;
    }
}
