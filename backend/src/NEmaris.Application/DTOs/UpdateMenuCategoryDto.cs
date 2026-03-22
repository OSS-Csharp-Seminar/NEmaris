using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class UpdateMenuCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public TimeSpan AvailableFrom { get; set; }

    public TimeSpan AvailableTo { get; set; }

    public bool IsActive { get; set; }
}