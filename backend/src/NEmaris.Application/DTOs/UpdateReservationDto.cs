using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class UpdateReservationDto
{
    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [Range(1, 100)]
    public int? PartySize { get; set; }

    [MaxLength(20)]
    public string? TableNumber { get; set; }

    [MaxLength(2000)]
    public string? SpecialRequest { get; set; }
}
