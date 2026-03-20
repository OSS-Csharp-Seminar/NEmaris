using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _environment;
    private readonly AppDbContext _db;

    public TokenService(IConfiguration config, IHostEnvironment environment, AppDbContext db)
    {
        _config = config;
        _environment = environment;
        _db = db;
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

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshAsync(string token)
    {
        var existing = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        // Rotate: revoke old, issue new
        existing.RevokedAt = DateTime.UtcNow;

        var newRefresh = new RefreshToken
        {
            UserId = existing.UserId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = existing.ExpiresAt 
        };

        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(existing.User);
        return (accessToken, newRefresh);
    }

    public async Task RevokeAsync(string token)
    {
        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        existing.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
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