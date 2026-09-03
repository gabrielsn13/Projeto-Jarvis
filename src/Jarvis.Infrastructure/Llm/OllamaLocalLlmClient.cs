using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Jarvis.Application.Abstractions;
using Jarvis.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Llm;

public sealed class OllamaLocalLlmClient(
    HttpClient httpClient,
    IOptions<JarvisOptions> options,
    ILogger<OllamaLocalLlmClient> logger) : ILocalLlmClient
{
    private readonly JarvisOptions _options = options.Value;

    public async Task<string> GenerateReplyAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest
        {
            Model = _options.Model,
            Stream = false,
            Messages = messages.Select(m => new OllamaMessage { Role = m.Role, Content = m.Content }).ToList()
        };

        try
        {
            using var response = await httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
            return body?.Message?.Content?.Trim() ?? "Desculpe, não consegui gerar uma resposta.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao consultar Ollama local.");
            return "Não consegui acessar o Ollama agora. Verifique se ele está em execução.";
        }
    }

    private sealed class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("stream")]
        public required bool Stream { get; init; }

        [JsonPropertyName("messages")]
        public required List<OllamaMessage> Messages { get; init; }
    }

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; init; }
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; init; }

        [JsonPropertyName("content")]
        public required string Content { get; init; }
    }
}
