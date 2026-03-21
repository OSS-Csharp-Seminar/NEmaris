using NEmaris.Domain.Enums;

namespace NEmaris.Domain.Entities;

public class Reservations
{
    public long Id { get; set; }
    public long GuestId { get; set; }
    public long TableId { get; set; }
    public string? ReservedByUserId { get; set; }
    public DateOnly ReservationDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
    public string? SpecialRequest { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guests Guest { get; set; } = null!;
    public RestaurantTables Table { get; set; } = null!;
    public ApplicationUser? ReservedByUser { get; set; }
}
