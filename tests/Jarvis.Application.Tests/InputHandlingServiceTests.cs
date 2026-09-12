using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Jarvis.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jarvis.Application.Tests;

public sealed class InputHandlingServiceTests
{
    [Fact]
    public async Task HandleAsync_ShouldNotCallChatService_WhenIntentIsUnknownCommand()
    {
        var chatService = new FakeChatService();
        var commandService = new FakeCommandService();
        var service = new InputHandlingService(
            new IntentRouter(new AllowlistCommandCatalog()),
            chatService,
            commandService,
            NullLogger<InputHandlingService>.Instance);

        var result = await service.HandleAsync("default", "/cmd comando_inexistente");

        Assert.Equal(InputHandlingDecision.Blocked, result.Decision);
        Assert.Equal("Comando inválido ou bloqueado. Use /cmd <comando>", result.Message);
        Assert.Equal(0, chatService.CallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallChatService_WhenIntentIsChat()
    {
        var chatService = new FakeChatService();
        var commandService = new FakeCommandService();
        var service = new InputHandlingService(
            new IntentRouter(new AllowlistCommandCatalog()),
            chatService,
            commandService,
            NullLogger<InputHandlingService>.Instance);

        var result = await service.HandleAsync("default", "Olá, Jarvis!");

        Assert.Equal(InputHandlingDecision.Chat, result.Decision);
        Assert.Equal("resposta teste", result.Message);
        Assert.Equal(1, chatService.CallCount);
    }

    private sealed class FakeChatService : IChatService
    {
        public int CallCount { get; private set; }

        public Task<ChatResponse> SendMessageAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new ChatResponse { Content = "resposta teste" });
        }
    }

    private sealed class FakeCommandService : ICommandService
    {
        public Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = CommandExecutionStatus.Success,
                Message = "Comando executado com sucesso."
            });
        }
    }
}
