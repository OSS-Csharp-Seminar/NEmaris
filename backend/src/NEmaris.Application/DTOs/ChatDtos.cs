using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class ChatRequestDto
{
    [Required]
    [MinLength(1)]
    public List<ChatMessageDto> Messages { get; set; } = new();

    public string? TimeZone { get; set; }

    public string? SessionId { get; set; }
}

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public bool ReservationsChanged { get; set; }
}
