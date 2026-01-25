using backend_project.Models;
using System.Security.Claims;

namespace backend_project.Services.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token containing user claims, roles, and permissions
    /// </summary>
    Task<string> GenerateAccessTokenAsync(User user, Guid sessionId);

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Hashes a token using SHA256 for secure storage
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Validates JWT token structure and expiration
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Extracts claims from token without full validation (for refresh scenarios)
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromToken(string token);

    /// <summary>
    /// Extracts session ID from JWT token
    /// </summary>
    Guid? GetSessionIdFromToken(string token);
}
