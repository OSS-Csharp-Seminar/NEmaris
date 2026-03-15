using NEmaris.Domain.Entities;

namespace NEmaris.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}