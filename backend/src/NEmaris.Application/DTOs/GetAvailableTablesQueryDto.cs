using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class GetAvailableTablesQueryDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Range(1, 100)]
    public int PartySize { get; set; }
}
