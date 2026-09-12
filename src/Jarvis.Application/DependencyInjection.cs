using Jarvis.Application.Abstractions;
using Jarvis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddJarvisApplication(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ICommandService, CommandService>();
        services.AddScoped<IInputHandlingService, InputHandlingService>();
        services.AddSingleton<IIntentRouter, IntentRouter>();
        services.AddSingleton<ICommandCatalog, AllowlistCommandCatalog>();
        return services;
    }
}
