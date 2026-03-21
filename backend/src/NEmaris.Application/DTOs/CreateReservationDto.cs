using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class CreateReservationDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    public string? Notes { get; set; }

    [Range(1, long.MaxValue)]
    public long TableId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Range(1, 100)]
    public int PartySize { get; set; }

    public string? SpecialRequest { get; set; }
}
