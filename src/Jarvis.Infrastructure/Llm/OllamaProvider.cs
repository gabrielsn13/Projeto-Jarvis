using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Application.Abstractions;
using Jarvis.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Llm;

public sealed class OllamaProvider(
    HttpClient httpClient,
    IOptions<OllamaOptions> options,
    ILogger<OllamaProvider> logger) : ILLMProvider
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> GenerateResponseAsync(string prompt, IReadOnlyList<ChatMessage> context, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var payload = new OllamaGenerateRequest
        {
            Model = _options.Model,
            Prompt = prompt,
            Stream = false
        };

        var startedAt = DateTime.UtcNow;

        try
        {
            logger.LogInformation(
                "Enviando requisição ao Ollama. Model={Model}; ContextMessages={ContextMessages}; PromptLength={PromptLength}",
                _options.Model,
                context.Count,
                prompt.Length);

            using var response = await httpClient.PostAsJsonAsync("/api/generate", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Ollama retornou status inválido. StatusCode={StatusCode}; Model={Model}",
                    (int)response.StatusCode,
                    _options.Model);
                throw new InvalidOperationException("Não foi possível obter resposta do Ollama no momento.");
            }

            var body = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(body?.Response))
            {
                logger.LogWarning("Resposta inválida do Ollama recebida para o modelo {Model}.", _options.Model);
                throw new InvalidOperationException("O Ollama retornou uma resposta inválida.");
            }

            logger.LogInformation(
                "Resposta do Ollama recebida com sucesso. Model={Model}; DurationMs={DurationMs}",
                _options.Model,
                (DateTime.UtcNow - startedAt).TotalMilliseconds);

            return body.Response.Trim();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Timeout ao consultar Ollama. Model={Model}; TimeoutSeconds={TimeoutSeconds}", _options.Model, _options.TimeoutSeconds);
            throw new InvalidOperationException("O Ollama demorou para responder. Tente novamente em instantes.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Ollama indisponível em {BaseUrl}.", _options.BaseUrl);
            throw new InvalidOperationException("Não foi possível conectar ao Ollama. Verifique se ele está ativo.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Falha ao interpretar resposta do Ollama. Model={Model}", _options.Model);
            throw new InvalidOperationException("O Ollama retornou um formato de resposta inválido.");
        }
    }

    private sealed class OllamaGenerateRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("prompt")]
        public required string Prompt { get; init; }

        [JsonPropertyName("stream")]
        public required bool Stream { get; init; }
    }

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; init; }
    }
}
