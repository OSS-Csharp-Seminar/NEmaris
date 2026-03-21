namespace NEmaris.Application.DTOs;

public class AvailableTableDto
{
    public long Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Zone { get; set; }
}
