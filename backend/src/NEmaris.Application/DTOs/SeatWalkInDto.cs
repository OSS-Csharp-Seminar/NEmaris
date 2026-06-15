using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class SeatWalkInDto
{
    [Required]
    [Range(1, 100)]
    public int GuestCount { get; set; }
}
