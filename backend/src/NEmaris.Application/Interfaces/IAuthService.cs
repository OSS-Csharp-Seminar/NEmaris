using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}