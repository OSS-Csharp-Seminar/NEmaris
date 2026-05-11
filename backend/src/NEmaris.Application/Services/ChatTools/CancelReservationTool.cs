using System.Text.Json;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class CancelReservationTool : IChatTool
{
    private readonly IReservationService _reservationService;

    public CancelReservationTool(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public string Name => "cancel_reservation";

    public string Description =>
        "Cancel a reservation. Identify it by the guest's phone number plus the reservation's startTime. " +
        "The phone must match the reservation's guest phone, otherwise the cancellation is rejected.";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            phone = new { type = "string" },
            startTime = new { type = "string", description = "The reservation's startTime as a full ISO 8601 datetime, taken from find_my_reservations." }
        },
        required = new[] { "phone", "startTime" }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var phone = ToolArgs.GetString(arguments, "phone");
        var startTime = ToolArgs.GetDateTime(arguments, "startTime");

        var existing = await ReservationLookup.ResolveActiveByPhoneAndStartAsync(_reservationService, phone, startTime);
        var cancelled = await _reservationService.CancelReservationAsync(existing.Id, phone);

        var summary = new
        {
            tableNumber = cancelled.TableNumber,
            startTime = cancelled.StartTime,
            endTime = cancelled.EndTime,
            status = cancelled.Status
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }
}
