using System.ComponentModel.DataAnnotations;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.DTO_s;

public class UpdateTableDto
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
}
