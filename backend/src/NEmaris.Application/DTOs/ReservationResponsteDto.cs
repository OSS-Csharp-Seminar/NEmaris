using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTOs;

public class ReservationResponseDto
{
    public long Id { get; set; }
    public long GuestId { get; set; }
    public string GuestFullName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public long TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public DateOnly ReservationDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
    public string? SpecialRequest { get; set; }
    public string? ReservedByUserId { get; set; }
}
