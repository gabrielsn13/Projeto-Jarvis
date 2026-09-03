using Jarvis.Application.Abstractions;
using Jarvis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddJarvisApplication(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}
