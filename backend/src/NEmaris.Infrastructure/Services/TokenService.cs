using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;

namespace NEmaris.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _environment;
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public TokenService(IConfiguration config, IHostEnvironment environment, IRefreshTokenRepository refreshTokenRepo)
    {
        _config = config;
        _environment = environment;
        _refreshTokenRepo = refreshTokenRepo;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ResolveJwtKey()));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName",  user.LastName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var refreshDays = double.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        };

        await _refreshTokenRepo.AddAsync(refreshToken);
        return refreshToken;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshAsync(string token)
    {
        var existing = await _refreshTokenRepo.FindByTokenAsync(token)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        // Rotate: revoke old, issue new — atomically via repo
        existing.RevokedAt = DateTime.UtcNow;

        var newRefresh = new RefreshToken
        {
            UserId = existing.UserId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = existing.ExpiresAt
        };

        await _refreshTokenRepo.RotateAsync(existing, newRefresh);

        var accessToken = GenerateAccessToken(existing.User);
        return (accessToken, newRefresh);
    }

    public async Task RevokeAsync(string token)
    {
        var existing = await _refreshTokenRepo.FindByTokenAsync(token)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        existing.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepo.UpdateAsync(existing);
    }

    private string ResolveJwtKey()
    {
        const string fallback = "NEmaris-Development-JWT-Key-AtLeast-32Chars-2026";
        var key = _config["Jwt:Key"];
        var resolved = string.IsNullOrWhiteSpace(key) && _environment.IsDevelopment() ? fallback : key;

        if (string.IsNullOrWhiteSpace(resolved))
            throw new InvalidOperationException("JWT key is missing.");
        if (resolved.Length < 32)
            throw new InvalidOperationException("JWT key is too short.");

        return resolved;
    }
}
