namespace NEmaris.Application.DTOs;

public class MenuItemDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Status { get; set; }
    public bool IsAvailable { get; set; }
    public string? Sku { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}