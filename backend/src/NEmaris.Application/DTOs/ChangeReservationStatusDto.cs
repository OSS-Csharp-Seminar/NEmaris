using System.ComponentModel.DataAnnotations;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTOs;

public class ChangeReservationStatusDto
{
    [Required]
    public ReservationStatus Status { get; set; }
}
