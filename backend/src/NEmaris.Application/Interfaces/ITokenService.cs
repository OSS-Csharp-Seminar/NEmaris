using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user);
    Task<(string AccessToken, RefreshToken RefreshToken)> RefreshAsync(string token);
    Task RevokeAsync(string token);
}