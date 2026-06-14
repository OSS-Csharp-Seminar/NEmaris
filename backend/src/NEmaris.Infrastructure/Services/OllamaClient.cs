using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NEmaris.Application.Configuration;
using NEmaris.Application.Interfaces;

namespace NEmaris.Infrastructure.Services;

public class OllamaClient : IOllamaClient
{
    private const int MaxAttempts = 8;
    private const double RetryTemperature = 0.2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<OllamaChatMessage> ChatAsync(
        IReadOnlyList<OllamaChatMessage> messages,
        IReadOnlyList<OllamaToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        var payload = await PostWithRetryAsync(messages, tools, cancellationToken);
        var message = payload.Message ?? throw new InvalidOperationException("Ollama response missing message.");

        var toolCalls = message.ToolCalls?
            .Select(tc => new OllamaToolCall
            {
                Name = tc.Function.Name,
                Arguments = tc.Function.Arguments
            })
            .ToList();

        return new OllamaChatMessage
        {
            Role = message.Role,
            Content = message.Content ?? string.Empty,
            ToolCalls = toolCalls is { Count: > 0 } ? toolCalls : null
        };
    }

    private async Task<OllamaChatResponse> PostWithRetryAsync(
        IReadOnlyList<OllamaChatMessage> messages,
        IReadOnlyList<OllamaToolDefinition> tools,
        CancellationToken cancellationToken)
    {
        HttpRequestException? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var temperature = attempt == 1 ? _options.Temperature : RetryTemperature;

            var request = BuildRequest(messages, tools, temperature);

            using var response = await _httpClient.PostAsJsonAsync("/api/chat", request, JsonOptions, cancellationToken);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                Console.WriteLine($"[ollama-retry] attempt {attempt} got 500 at temp={temperature:0.00}");
                lastException = new HttpRequestException(
                    $"Ollama returned 500 on attempt {attempt}.",
                    null,
                    HttpStatusCode.InternalServerError);

                if (attempt < MaxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), cancellationToken);
                    continue;
                }
                break;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Empty response from Ollama.");
        }

        if (tools.Count > 0)
        {
            Console.WriteLine("[ollama-retry] all attempts failed; attempting no-tools fallback");
            var fallbackRequest = BuildRequest(messages, Array.Empty<OllamaToolDefinition>(), RetryTemperature);
            using var fallbackResponse = await _httpClient.PostAsJsonAsync("/api/chat", fallbackRequest, JsonOptions, cancellationToken);
            if (fallbackResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("[ollama-retry] no-tools fallback succeeded");
                return await fallbackResponse.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken)
                    ?? throw new InvalidOperationException("Empty response from Ollama fallback.");
            }
            Console.WriteLine($"[ollama-retry] no-tools fallback also failed with {(int)fallbackResponse.StatusCode}");
        }

        if (lastException is not null) throw lastException;
        throw new InvalidOperationException("Ollama call failed after retries with no captured exception.");
    }

    private object BuildRequest(
        IReadOnlyList<OllamaChatMessage> messages,
        IReadOnlyList<OllamaToolDefinition> tools,
        double temperature) => new
    {
        model = _options.Model,
        stream = false,
        think = false,
        options = new
        {
            temperature,
            num_ctx = _options.NumCtx
        },
        messages = messages.Select(m => new
        {
            role = m.Role,
            content = m.Content
        }).ToList(),
        tools = tools.Select(t => new
        {
            type = "function",
            function = new
            {
                name = t.Name,
                description = t.Description,
                parameters = t.Parameters
            }
        }).ToList()
    };

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCallWire>? ToolCalls { get; set; }
    }

    private sealed class OllamaToolCallWire
    {
        [JsonPropertyName("function")]
        public OllamaFunctionCallWire Function { get; set; } = new();
    }

    private sealed class OllamaFunctionCallWire
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; }
    }
}
