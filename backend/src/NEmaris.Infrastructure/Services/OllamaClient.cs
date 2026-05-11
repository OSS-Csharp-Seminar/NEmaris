using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NEmaris.Application.Configuration;
using NEmaris.Application.Interfaces;

namespace NEmaris.Infrastructure.Services;

public class OllamaClient : IOllamaClient
{
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
        var request = new
        {
            model = _options.Model,
            stream = false,
            options = new { temperature = _options.Temperature },
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

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Ollama.");

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
