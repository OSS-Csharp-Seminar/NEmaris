namespace NEmaris.Domain.Entities;

public class MenuCategory
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public TimeSpan AvailableFrom { get; set; }
    public TimeSpan AvailableTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}