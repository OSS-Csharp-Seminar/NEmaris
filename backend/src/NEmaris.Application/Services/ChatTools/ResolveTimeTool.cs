using System.Text.Json;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class ResolveTimeTool : IChatTool
{
    private readonly IRequestTimeZoneContext _timeZoneContext;

    public ResolveTimeTool(IRequestTimeZoneContext timeZoneContext)
    {
        _timeZoneContext = timeZoneContext;
    }

    public string Name => "resolve_time";

    public string Description =>
        "Convert a guest's time phrase into a canonical UTC startTime. " +
        "ALWAYS call this FIRST whenever the guest mentions a time, BEFORE calling get_available_tables or create_reservation. " +
        "Pass ONE of: " +
        "(a) offsetMinutes — the number of minutes from now (e.g. 5 for \"in 5 minutes\", 90 for \"in 90 minutes\", 120 for \"in 2 hours\"); " +
        "(b) localClock — an absolute time the guest gave you (e.g. \"tonight at 8pm\" → day=\"today\", hour=20, minute=0; " +
        "\"tomorrow at 7pm\" → day=\"tomorrow\", hour=19, minute=0; \"next Friday at 19:00\" → day=\"friday\", hour=19, minute=0). " +
        "The tool returns startTimeUtc — copy this value VERBATIM into all subsequent tool calls. Never compute UTC times yourself.";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            offsetMinutes = new
            {
                type = "integer",
                description = "Minutes from current time. Use for relative phrases like \"now\" (0), \"in 5 minutes\" (5), \"in an hour\" (60), \"in 90 minutes\" (90), \"in 2 hours\" (120). Omit if using localClock."
            },
            localClock = new
            {
                type = "object",
                description = "Absolute time in the guest's local timezone. Use for phrases like \"tonight at 8pm\", \"tomorrow at noon\", \"next Friday at 7pm\". Omit if using offsetMinutes.",
                properties = new
                {
                    day = new
                    {
                        type = "string",
                        description = "\"today\", \"tomorrow\", or a weekday name (\"monday\", \"tuesday\", \"wednesday\", \"thursday\", \"friday\", \"saturday\", \"sunday\"). Weekdays mean the next occurrence after today."
                    },
                    hour = new { type = "integer", description = "0-23 in the guest's local timezone. \"8pm\" = 20, \"noon\" = 12, \"midnight\" = 0." },
                    minute = new { type = "integer", description = "0-59. Default 0 if the guest didn't specify minutes." }
                },
                required = new[] { "day", "hour" }
            }
        }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var tz = _timeZoneContext.TimeZone;
        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

        DateTime resolvedUtc;
        string interpretation;

        var hasOffset = arguments.TryGetProperty("offsetMinutes", out var offsetProp)
            && offsetProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined);

        var hasClock = arguments.TryGetProperty("localClock", out var clockProp)
            && clockProp.ValueKind is JsonValueKind.Object;

        if (hasOffset && hasClock)
            throw new InvalidOperationException("Pass either offsetMinutes OR localClock, not both.");

        if (!hasOffset && !hasClock)
            throw new InvalidOperationException("Pass either offsetMinutes (for relative phrases) or localClock (for absolute times).");

        if (hasOffset)
        {
            var offset = ToolArgs.GetInt32(arguments, "offsetMinutes");
            resolvedUtc = nowUtc.AddMinutes(offset);
            interpretation = offset == 0
                ? "now"
                : offset > 0
                    ? $"now + {offset} minutes"
                    : $"now - {Math.Abs(offset)} minutes";
        }
        else
        {
            var day = ToolArgs.GetString(clockProp, "day").Trim().ToLowerInvariant();
            var hour = ToolArgs.GetInt32(clockProp, "hour");
            var minute = clockProp.TryGetProperty("minute", out var minProp)
                && minProp.ValueKind is JsonValueKind.Number
                ? minProp.GetInt32()
                : 0;

            if (hour is < 0 or > 23)
                throw new InvalidOperationException("hour must be between 0 and 23.");
            if (minute is < 0 or > 59)
                throw new InvalidOperationException("minute must be between 0 and 59.");

            var targetDate = ResolveDay(nowLocal.Date, day);
            var targetLocal = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, hour, minute, 0, DateTimeKind.Unspecified);
            resolvedUtc = TimeZoneInfo.ConvertTimeToUtc(targetLocal, tz);
            interpretation = $"{day} at {hour:00}:{minute:00} local ({tz.Id})";
        }

        var resolvedLocal = TimeZoneInfo.ConvertTimeFromUtc(resolvedUtc, tz);
        var minutesFromNow = (int)Math.Round((resolvedUtc - nowUtc).TotalMinutes);

        var summary = new
        {
            startTimeUtc = resolvedUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            startTimeLocal = resolvedLocal.ToString("yyyy-MM-dd HH:mm"),
            timeZone = tz.Id,
            minutesFromNow,
            interpretation,
            reminder = "Copy startTimeUtc verbatim into get_available_tables and create_reservation. Do not recompute it."
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }

    private static DateTime ResolveDay(DateTime today, string day)
    {
        return day switch
        {
            "today" or "" => today,
            "tomorrow" => today.AddDays(1),
            "yesterday" => today.AddDays(-1),
            "monday" => NextWeekday(today, DayOfWeek.Monday),
            "tuesday" => NextWeekday(today, DayOfWeek.Tuesday),
            "wednesday" => NextWeekday(today, DayOfWeek.Wednesday),
            "thursday" => NextWeekday(today, DayOfWeek.Thursday),
            "friday" => NextWeekday(today, DayOfWeek.Friday),
            "saturday" => NextWeekday(today, DayOfWeek.Saturday),
            "sunday" => NextWeekday(today, DayOfWeek.Sunday),
            _ => throw new InvalidOperationException(
                $"day '{day}' is not recognized. Use \"today\", \"tomorrow\", or a weekday name.")
        };
    }

    private static DateTime NextWeekday(DateTime from, DayOfWeek target)
    {
        var daysAhead = ((int)target - (int)from.DayOfWeek + 7) % 7;
        if (daysAhead == 0) daysAhead = 7;
        return from.AddDays(daysAhead);
    }
}
