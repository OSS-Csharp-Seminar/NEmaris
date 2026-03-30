using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class CreateMenuItemDto
{
    [Required]
    public long CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Price { get; set; }

    public int Status { get; set; }

    public bool IsAvailable { get; set; } = true;

    [MaxLength(50)]
    public string? Sku { get; set; }
}