using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTO_s;

public class TableDto
{
    public long Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Zone { get; set; }
    public TableStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
