using System.Text.Json;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class FindReservationsByPhoneTool : IChatTool
{
    private readonly IReservationService _reservationService;

    public FindReservationsByPhoneTool(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public string Name => "find_my_reservations";

    public string Description =>
        "Look up a guest's UPCOMING active reservations. " +
        "Requires both phone number AND last name; both must match the booking. " +
        "Returns an empty list if either does not match — never confirm or deny which one was wrong.";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            phone = new { type = "string" },
            lastName = new { type = "string" }
        },
        required = new[] { "phone", "lastName" }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var phone = ToolArgs.GetString(arguments, "phone");
        var lastName = ToolArgs.GetString(arguments, "lastName");

        var reservations = await _reservationService.GetUpcomingReservationsForGuestAsync(phone, lastName);

        var summary = new
        {
            count = reservations.Count,
            reservations = reservations.Select(r => new
            {
                tableNumber = r.TableNumber,
                startTime = r.StartTime,
                endTime = r.EndTime,
                partySize = r.PartySize,
                specialRequest = r.SpecialRequest
            }).ToList()
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }
}
