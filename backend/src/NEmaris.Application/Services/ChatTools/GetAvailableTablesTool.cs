using System.Text.Json;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class GetAvailableTablesTool : IChatTool
{
    private const int DefaultDurationMinutes = 90;

    private readonly IReservationService _reservationService;

    public GetAvailableTablesTool(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public string Name => "get_available_tables";

    public string Description =>
        "List restaurant tables that fit the requested party size and are free for the requested time window. " +
        "Always call this first when a guest wants to make a reservation, then pick a table from the result. " +
        "Provide startTime as a full ISO 8601 datetime including the date. " +
        "Either pass endTime (also full ISO datetime) or durationMinutes (defaults to 90).";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            startTime = new { type = "string", description = "Full ISO 8601 datetime, e.g. 2026-05-10T19:00:00" },
            endTime = new { type = "string", description = "Full ISO 8601 datetime; omit if you provide durationMinutes" },
            durationMinutes = new { type = "integer", description = "Length of the reservation in minutes; defaults to 90" },
            partySize = new { type = "integer", minimum = 1 }
        },
        required = new[] { "startTime", "partySize" }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var startTime = ToolArgs.GetDateTime(arguments, "startTime");
        var endTime = ResolveEndTime(arguments, startTime);

        var query = new GetAvailableTablesQueryDto
        {
            StartTime = startTime,
            EndTime = endTime,
            PartySize = ToolArgs.GetInt32(arguments, "partySize"),
            ExcludeReservationId = ToolArgs.GetOptionalInt64(arguments, "excludeReservationId")
        };

        var tables = await _reservationService.GetAvailableTablesAsync(query);

        var summary = new
        {
            available = tables.Count > 0,
            count = tables.Count,
            tables = tables.Select(t => new
            {
                tableNumber = t.TableNumber,
                capacity = t.Capacity,
                zone = t.Zone
            }).ToList()
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }

    internal static DateTime ResolveEndTime(JsonElement arguments, DateTime startTime)
    {
        var hasEndTime = arguments.TryGetProperty("endTime", out var endProp)
            && endProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            && !(endProp.ValueKind is JsonValueKind.String && string.IsNullOrWhiteSpace(endProp.GetString()));

        if (hasEndTime)
            return ToolArgs.GetDateTime(arguments, "endTime");

        var duration = ToolArgs.GetOptionalInt32(arguments, "durationMinutes") ?? DefaultDurationMinutes;
        return startTime.AddMinutes(duration);
    }
}
