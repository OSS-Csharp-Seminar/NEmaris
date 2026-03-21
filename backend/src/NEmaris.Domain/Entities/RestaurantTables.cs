using NEmaris.Domain.Enums;

namespace NEmaris.Domain.Entities;

public class RestaurantTables
{
    public long Id { get; set; }
    public required string TableNumber { get; set; }
    public int Capacity { get; set; }
    public string? Zone { get; set; }
    public TableStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Reservations> Reservations { get; set; } = [];
}
