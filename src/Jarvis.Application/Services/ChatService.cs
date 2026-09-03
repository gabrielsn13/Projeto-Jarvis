using Jarvis.Application.Abstractions;
using Jarvis.Application.Models;
using Jarvis.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Jarvis.Application.Services;

public sealed class ChatService(
    IChatRepository chatRepository,
    ILocalLlmClient localLlmClient,
    ILogger<ChatService> logger) : IChatService
{
    private const int ContextWindowSize = 8;

    public async Task<ChatResponse> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("A mensagem não pode ser vazia.", nameof(userMessage));
        }

        var userChatMessage = new ChatMessage
        {
            Role = "user",
            Content = userMessage.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await chatRepository.AddMessageAsync(userChatMessage, cancellationToken);

        var context = await chatRepository.GetRecentMessagesAsync(ContextWindowSize, cancellationToken);
        var reply = await localLlmClient.GenerateReplyAsync(context, cancellationToken);

        var assistantChatMessage = new ChatMessage
        {
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await chatRepository.AddMessageAsync(assistantChatMessage, cancellationToken);

        logger.LogInformation("Resposta gerada e persistida com sucesso.");

        return new ChatResponse { Content = reply };
    }
}
