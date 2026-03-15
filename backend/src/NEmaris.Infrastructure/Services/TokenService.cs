using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application.Auth.Interfaces;
using NEmaris.Domain.Entities;

namespace NEmaris.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _environment;

    public TokenService(IConfiguration config, IHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
    }

    public string GenerateToken(ApplicationUser user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var keyValue = ResolveJwtKey();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName",  user.LastName),
            new("role",      user.Role.ToString())
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

    private string ResolveJwtKey()
    {
        const string developmentFallbackKey = "NEmaris-Development-JWT-Key-AtLeast-32Chars-2026";

        var configuredKey = _config["Jwt:Key"];
        var jwtKey = string.IsNullOrWhiteSpace(configuredKey) && _environment.IsDevelopment()
            ? developmentFallbackKey
            : configuredKey;

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("JWT key is missing. Set Jwt:Key in configuration or Jwt__Key environment variable.");

        if (jwtKey.Length < 32)
            throw new InvalidOperationException("JWT key is too short. Use at least 32 characters.");

        return jwtKey;
    }
}
