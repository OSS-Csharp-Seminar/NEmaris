using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEmaris.Simulator;

internal sealed class GuestAgent
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly Uri _chatUri;
    private readonly string _model;
    private readonly int _numCtx;
    private readonly double _temperature;

    public GuestAgent(HttpClient http, string ollamaUrl, string model, int numCtx = 8192, double temperature = 0.7)
    {
        _http = http;
        _chatUri = new Uri(new Uri(ollamaUrl.TrimEnd('/') + "/"), "api/chat");
        _model = model;
        _numCtx = numCtx;
        _temperature = temperature;
    }

    public async Task<string> NextMessageAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<object>(history.Count + 1)
        {
            new { role = "system", content = systemPrompt }
        };
        foreach (var msg in history)
            messages.Add(new { role = msg.Role, content = msg.Content });

        var request = new
        {
            model = _model,
            stream = false,
            think = false,
            options = new { temperature = _temperature, num_ctx = _numCtx },
            messages
        };

        using var response = await _http.PostAsJsonAsync(_chatUri, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(JsonOptions, cancellationToken);
        return (payload?.Message?.Content ?? string.Empty).Trim();
    }

    private sealed class OllamaResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
