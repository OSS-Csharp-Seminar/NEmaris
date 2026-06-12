using System.Text.Json;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class UpdateReservationTool : IChatTool
{
    private readonly IReservationService _reservationService;

    public UpdateReservationTool(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public string Name => "update_reservation";

    public string Description =>
        "Change an existing active reservation. Identify it by the guest's phone plus the current startTime " +
        "(get the current startTime from find_my_reservations). " +
        "Pass only the fields the guest wants to change. " +
        "If changing the table or time, the new slot must be free — call get_available_tables first to confirm.";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            phone = new { type = "string" },
            startTime = new { type = "string", description = "The reservation's CURRENT startTime, taken from find_my_reservations. This identifies which reservation to change." },
            newStartTime = new { type = "string", description = "Optional. New start time as full ISO 8601 datetime; pass with newEndTime or durationMinutes." },
            newEndTime = new { type = "string", description = "Optional. New end time; or pass durationMinutes." },
            durationMinutes = new { type = "integer", description = "Optional. Used together with newStartTime to compute newEndTime." },
            partySize = new { type = "integer", minimum = 1, description = "Optional. New party size." },
            tableNumber = new { type = "string", description = "Optional. New table number from get_available_tables, e.g. \"T2\"." },
            specialRequest = new { type = "string", description = "Optional. Empty string clears it." }
        },
        required = new[] { "phone", "startTime" }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var phone = ToolArgs.GetString(arguments, "phone");
        var currentStart = ToolArgs.GetDateTime(arguments, "startTime");

        var existing = await ReservationLookup.ResolveByPhoneAndStartAsync(_reservationService, phone, currentStart);

        var dto = new UpdateReservationDto { Phone = phone };

        var hasNewStart = arguments.TryGetProperty("newStartTime", out var startProp) &&
            startProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined) &&
            !(startProp.ValueKind is JsonValueKind.String && string.IsNullOrWhiteSpace(startProp.GetString()));

        if (hasNewStart)
        {
            var start = ToolArgs.GetDateTime(arguments, "newStartTime");
            dto.StartTime = start;
            dto.EndTime = ResolveNewEndTime(arguments, start);
        }
        else if (arguments.TryGetProperty("newEndTime", out var endProp) &&
            endProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            dto.EndTime = ToolArgs.GetDateTime(arguments, "newEndTime");
        }

        dto.PartySize = ToolArgs.GetOptionalInt32(arguments, "partySize");

        var tableNumber = ToolArgs.GetOptionalString(arguments, "tableNumber");
        if (!string.IsNullOrWhiteSpace(tableNumber))
            dto.TableNumber = tableNumber;

        if (arguments.TryGetProperty("specialRequest", out var srProp) &&
            srProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            dto.SpecialRequest = srProp.GetString() ?? string.Empty;
        }

        var updated = await _reservationService.UpdateReservationAsync(existing.Id, dto);

        var summary = new
        {
            tableNumber = updated.TableNumber,
            startTime = updated.StartTime,
            endTime = updated.EndTime,
            partySize = updated.PartySize,
            status = updated.Status,
            specialRequest = updated.SpecialRequest
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }

    private static DateTime ResolveNewEndTime(JsonElement arguments, DateTime newStart)
    {
        var hasNewEnd = arguments.TryGetProperty("newEndTime", out var endProp) &&
            endProp.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined) &&
            !(endProp.ValueKind is JsonValueKind.String && string.IsNullOrWhiteSpace(endProp.GetString()));

        if (hasNewEnd)
            return ToolArgs.GetDateTime(arguments, "newEndTime");

        var duration = ToolArgs.GetOptionalInt32(arguments, "durationMinutes") ?? 90;
        return newStart.AddMinutes(duration);
    }
}
