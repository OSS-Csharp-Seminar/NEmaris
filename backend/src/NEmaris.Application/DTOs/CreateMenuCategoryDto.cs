using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class CreateMenuCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}