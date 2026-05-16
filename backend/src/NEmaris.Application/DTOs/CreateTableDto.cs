using System.ComponentModel.DataAnnotations;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTOs;

public class CreateTableDto
{
    [Required]
    [MaxLength(20)]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(100)]
    public string Zone { get; set; } = string.Empty;

    public TableStatus Status { get; set; } = TableStatus.Available;

    [Range(1, 3)]
    public int Floor { get; set; } = 1;

    [Range(0, 100)]
    public decimal PositionX { get; set; }

    [Range(0, 100)]
    public decimal PositionY { get; set; }

    public TableShape Shape { get; set; } = TableShape.Square;

    [Range(0, 359)]
    public int Rotation { get; set; }
}
