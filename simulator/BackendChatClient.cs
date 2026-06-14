using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEmaris.Simulator;

internal sealed class BackendChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly Uri _chatUri;

    public BackendChatClient(HttpClient http, string baseUrl)
    {
        _http = http;
        _chatUri = new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), "api/chat");
    }

    public async Task<string> SendAsync(
        IReadOnlyList<ChatMessage> messages,
        string timeZone,
        string? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest
        {
            Messages = messages
                .Select(m => new ChatMessage { Role = m.Role, Content = m.Content })
                .ToList(),
            TimeZone = timeZone,
            SessionId = sessionId
        };

        using var response = await _http.PostAsJsonAsync(_chatUri, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, cancellationToken);
        return payload?.Reply ?? string.Empty;
    }

    private sealed class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new();
        public string? TimeZone { get; set; }
        public string? SessionId { get; set; }
    }

    private sealed class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}
