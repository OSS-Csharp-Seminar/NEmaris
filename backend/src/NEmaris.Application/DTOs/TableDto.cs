using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTOs;

public class TableDto
{
    public long Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Zone { get; set; }
    public TableStatus Status { get; set; }
    public int Floor { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public TableShape Shape { get; set; }
    public int Rotation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
