using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request);
    Task RevokeAsync(RevokeRequestDto request);
}