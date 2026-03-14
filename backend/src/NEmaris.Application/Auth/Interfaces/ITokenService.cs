using NEmaris.Domain.Entities;

namespace NEmaris.Application.Auth.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}