using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task AddAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task RotateAsync(RefreshToken revokedToken, RefreshToken newToken);
}
