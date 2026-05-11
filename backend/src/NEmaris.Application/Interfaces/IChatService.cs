using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IChatService
{
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken);
}
