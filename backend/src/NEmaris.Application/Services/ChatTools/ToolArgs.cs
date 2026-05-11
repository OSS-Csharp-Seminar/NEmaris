using System.Globalization;
using System.Text.Json;

namespace NEmaris.Application.Services.ChatTools;

internal static class ToolArgs
{
    public static string GetString(JsonElement args, string name, string fallback = "")
    {
        if (!args.TryGetProperty(name, out var prop)) return fallback;
        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString() ?? fallback,
            JsonValueKind.Number => prop.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => fallback,
            _ => prop.GetRawText()
        };
    }

    public static string? GetOptionalString(JsonElement args, string name)
    {
        if (!args.TryGetProperty(name, out var prop)) return null;
        if (prop.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
        var value = GetString(args, name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static int GetInt32(JsonElement args, string name)
    {
        if (!args.TryGetProperty(name, out var prop))
            throw new InvalidOperationException($"Missing required argument '{name}'.");

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.GetInt32(),
            JsonValueKind.String when int.TryParse(prop.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => throw new InvalidOperationException($"Argument '{name}' must be an integer.")
        };
    }

    public static long GetInt64(JsonElement args, string name)
    {
        if (!args.TryGetProperty(name, out var prop))
            throw new InvalidOperationException($"Missing required argument '{name}'.");

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.GetInt64(),
            JsonValueKind.String when long.TryParse(prop.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => throw new InvalidOperationException($"Argument '{name}' must be an integer.")
        };
    }

    public static int? GetOptionalInt32(JsonElement args, string name)
    {
        if (!args.TryGetProperty(name, out var prop)) return null;
        if (prop.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
        return GetInt32(args, name);
    }

    public static DateTime GetDateTime(JsonElement args, string name)
    {
        var raw = GetOptionalString(args, name)
            ?? throw new InvalidOperationException($"Missing required argument '{name}'.");

        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            throw new InvalidOperationException(
                $"Argument '{name}' must be a full ISO 8601 datetime, e.g. 2026-05-10T19:00:00. Got '{raw}'.");

        if (parsed.Date == DateTime.MinValue.Date)
            throw new InvalidOperationException(
                $"Argument '{name}' is missing a date. Provide a full datetime like 2026-05-10T19:00:00.");

        return parsed;
    }
}
