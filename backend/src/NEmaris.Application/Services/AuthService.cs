using Microsoft.AspNetCore.Identity;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Guest,
            Status = UserStatus.Active
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return new RegisterResponseDto("Account created successfully.");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Status == UserStatus.Banned)
            throw new UnauthorizedAccessException("This account has been banned.");

        if (user.Status == UserStatus.Inactive)
            throw new UnauthorizedAccessException("This account is inactive.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new AuthResponseDto(
            accessToken,
            refreshToken.Token
        );
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request)
    {
        var (accessToken, newRefresh) = await _tokenService.RefreshAsync(request.RefreshToken);

        var user = await _userManager.FindByIdAsync(newRefresh.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        return new AuthResponseDto(
            accessToken,
            newRefresh.Token
        );
    }

    public async Task RevokeAsync(RevokeRequestDto request)
        => await _tokenService.RevokeAsync(request.RefreshToken);

    private async Task<RegisterResponseDto> BuildResponseAsync(ApplicationUser user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new RegisterResponseDto("Account created successfully.");
    }
}