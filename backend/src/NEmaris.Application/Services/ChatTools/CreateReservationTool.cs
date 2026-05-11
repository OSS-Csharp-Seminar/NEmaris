using System.Text.Json;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.Application.Services.ChatTools;

public class CreateReservationTool : IChatTool
{
    private readonly IReservationService _reservationService;
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationTool(
        IReservationService reservationService,
        IReservationRepository reservationRepository)
    {
        _reservationService = reservationService;
        _reservationRepository = reservationRepository;
    }

    public string Name => "create_reservation";

    public string Description =>
        "Create a reservation against a tableNumber returned by get_available_tables. " +
        "Only call after the guest confirms the chosen time, party size, and table. " +
        "startTime must be a full ISO 8601 datetime; pass either endTime or durationMinutes (defaults to 90).";

    public object ParameterSchema => new
    {
        type = "object",
        properties = new
        {
            firstName = new { type = "string" },
            lastName = new { type = "string" },
            phone = new { type = "string" },
            email = new { type = "string" },
            tableNumber = new { type = "string", description = "The tableNumber from get_available_tables (e.g. \"T2\")" },
            startTime = new { type = "string", description = "Full ISO 8601 datetime" },
            endTime = new { type = "string", description = "Full ISO 8601 datetime; omit if you provide durationMinutes" },
            durationMinutes = new { type = "integer", description = "Length of the reservation in minutes; defaults to 90" },
            partySize = new { type = "integer", minimum = 1 },
            specialRequest = new { type = "string" },
            notes = new { type = "string" }
        },
        required = new[] { "firstName", "lastName", "phone", "tableNumber", "startTime", "partySize" }
    };

    public async Task<string> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var startTime = ToolArgs.GetDateTime(arguments, "startTime");
        var endTime = GetAvailableTablesTool.ResolveEndTime(arguments, startTime);

        var tableNumber = ToolArgs.GetString(arguments, "tableNumber").Trim();
        if (string.IsNullOrEmpty(tableNumber))
            throw new InvalidOperationException("tableNumber is required.");

        var table = await _reservationRepository.GetTableByNumberAsync(tableNumber)
            ?? throw new KeyNotFoundException($"No table named '{tableNumber}' exists.");

        var dto = new CreateReservationDto
        {
            FirstName = ToolArgs.GetString(arguments, "firstName"),
            LastName = ToolArgs.GetString(arguments, "lastName"),
            Phone = ToolArgs.GetString(arguments, "phone"),
            Email = ToolArgs.GetOptionalString(arguments, "email"),
            TableId = table.Id,
            StartTime = startTime,
            EndTime = endTime,
            PartySize = ToolArgs.GetInt32(arguments, "partySize"),
            SpecialRequest = ToolArgs.GetOptionalString(arguments, "specialRequest"),
            Notes = ToolArgs.GetOptionalString(arguments, "notes")
        };

        var reservation = await _reservationService.CreateReservationAsync(dto, reservedByUserId: null);

        var summary = new
        {
            tableNumber = reservation.TableNumber,
            startTime = reservation.StartTime,
            endTime = reservation.EndTime,
            partySize = reservation.PartySize,
            status = reservation.Status,
            specialRequest = reservation.SpecialRequest
        };

        return JsonSerializer.Serialize(summary, ToolJsonOptions.Default);
    }
}
