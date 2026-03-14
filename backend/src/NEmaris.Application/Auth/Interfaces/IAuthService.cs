using NEmaris.Application.Auth.DTOs;

namespace NEmaris.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}