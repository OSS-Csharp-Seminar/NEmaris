using System.Text.Json;

namespace NEmaris.Application.Interfaces;

public class OllamaToolCall
{
    public string Name { get; set; } = string.Empty;
    public JsonElement Arguments { get; set; }
}

public class OllamaChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<OllamaToolCall>? ToolCalls { get; set; }
}

public class OllamaToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object Parameters { get; set; } = new { };
}

public interface IOllamaClient
{
    Task<OllamaChatMessage> ChatAsync(
        IReadOnlyList<OllamaChatMessage> messages,
        IReadOnlyList<OllamaToolDefinition> tools,
        CancellationToken cancellationToken = default);
}
