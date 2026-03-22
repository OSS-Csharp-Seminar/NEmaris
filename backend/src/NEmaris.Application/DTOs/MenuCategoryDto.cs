namespace NEmaris.Application.DTOs;

public class MenuCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan AvailableFrom { get; set; }
    public TimeSpan AvailableTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}