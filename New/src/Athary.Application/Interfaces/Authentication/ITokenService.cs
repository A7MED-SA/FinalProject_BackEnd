using System.Security.Claims;
using Athary.Domain.Entities;

namespace Athary.Application.Interfaces.Authentication;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(User user, Guid sessionId, CancellationToken cancellationToken = default);
    string GenerateRefreshToken();
    string HashToken(string token);
    bool ValidateToken(string token);
    ClaimsPrincipal? GetPrincipalFromToken(string token);
    Guid? GetSessionIdFromToken(string token);
}
