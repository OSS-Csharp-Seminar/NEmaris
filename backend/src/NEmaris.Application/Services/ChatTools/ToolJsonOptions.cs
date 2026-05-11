using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEmaris.Application.Services.ChatTools;

internal static class ToolJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
