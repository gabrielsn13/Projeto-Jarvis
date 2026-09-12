using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Jarvis.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Jarvis.Application.Services;

public sealed class ChatService(
    IChatHistoryRepository chatHistoryRepository,
    ILLMProvider llmProvider,
    ILogger<ChatService> logger) : IChatService
{
    private const int ContextWindowSize = 10;

    public async Task<ChatResponse> SendMessageAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A sessão é obrigatória.", nameof(sessionId));
        }

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("A mensagem não pode ser vazia.", nameof(userMessage));
        }

        var trimmedMessage = userMessage.Trim();
        var history = await chatHistoryRepository.GetRecentMessagesAsync(sessionId, ContextWindowSize, cancellationToken);
        var prompt = BuildPrompt(history, trimmedMessage);
        var reply = await llmProvider.GenerateResponseAsync(prompt, history, cancellationToken);

        var userChatMessage = new ChatMessage
        {
            Role = "user",
            Content = trimmedMessage,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var assistantChatMessage = new ChatMessage
        {
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await chatHistoryRepository.AddMessageAsync(sessionId, userChatMessage, cancellationToken);
        await chatHistoryRepository.AddMessageAsync(sessionId, assistantChatMessage, cancellationToken);

        logger.LogInformation("Resposta gerada e persistida com sucesso para a sessão {SessionId}.", sessionId);

        return new ChatResponse { Content = reply };
    }

    private static string BuildPrompt(IReadOnlyList<ChatMessage> history, string userMessage)
    {
        var lines = new List<string>(history.Count + 1);
        foreach (var message in history)
        {
            lines.Add($"{message.Role}: {message.Content}");
        }

        lines.Add($"user: {userMessage}");
        return string.Join(Environment.NewLine, lines);
    }
}
