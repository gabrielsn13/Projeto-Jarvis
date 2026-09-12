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

    [Fact]
    public async Task HandleAsync_ShouldCallCommandService_WhenIntentIsValidCommand()
    {
        var chatService = new FakeChatService();
        var commandService = new FakeCommandService();
        var service = new InputHandlingService(
            new IntentRouter(new AllowlistCommandCatalog()),
            chatService,
            commandService,
            NullLogger<InputHandlingService>.Instance);

        var result = await service.HandleAsync("default", "/CMD ABRIR_NOTEPAD");

        Assert.Equal(InputHandlingDecision.Executed, result.Decision);
        Assert.Equal("Comando executado com sucesso.", result.Message);
        Assert.Equal(1, commandService.CallCount);
        Assert.Equal("abrir_notepad", commandService.LastCommandId);
    }

    [Fact]
    public async Task HandleAsync_ShouldPrefixSuccessMessage_WhenCommandReturnsCustomSuccessMessage()
    {
        var chatService = new FakeChatService();
        var commandService = new FakeCommandService(CommandExecutionStatus.Success, "Data e hora locais: 10/09/2026 10:00:00");
        var service = new InputHandlingService(
            new IntentRouter(new AllowlistCommandCatalog()),
            chatService,
            commandService,
            NullLogger<InputHandlingService>.Instance);

        var result = await service.HandleAsync("default", "/cmd mostrar_data_hora");

        Assert.Equal("Comando executado com sucesso. Data e hora locais: 10/09/2026 10:00:00", result.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotDuplicatePrefix_WhenCommandAlreadyReturnsPrefixedSuccessMessage()
    {
        var chatService = new FakeChatService();
        var commandService = new FakeCommandService(CommandExecutionStatus.Success, "Comando executado com sucesso. Data e hora locais: 10/09/2026 10:00:00");
        var service = new InputHandlingService(
            new IntentRouter(new AllowlistCommandCatalog()),
            chatService,
            commandService,
            NullLogger<InputHandlingService>.Instance);

        var result = await service.HandleAsync("default", "/cmd mostrar_data_hora");

        Assert.Equal("Comando executado com sucesso. Data e hora locais: 10/09/2026 10:00:00", result.Message);
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

    private sealed class FakeCommandService(
        CommandExecutionStatus status = CommandExecutionStatus.Success,
        string message = "Comando executado com sucesso.") : ICommandService
    {
        public int CallCount { get; private set; }
        public string? LastCommandId { get; private set; }

        public Task<CommandExecutionResult> ExecuteAsync(string commandId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCommandId = commandId;

            return Task.FromResult(new CommandExecutionResult
            {
                CommandId = commandId,
                Status = status,
                Message = message
            });
        }
    }
}
