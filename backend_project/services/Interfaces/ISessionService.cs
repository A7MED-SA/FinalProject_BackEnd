using backend_project.DTOs.Auth;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface ISessionService
{
    /// <summary>
    /// Creates a new session with hashed tokens
    /// </summary>
    Task<Session> CreateSessionAsync(
        User user,
        string refreshTokenHash,
        string ipAddress,
        string userAgent);

    /// <summary>
    /// Validates session is active and matches security context (IP, UserAgent)
    /// </summary>
    Task<bool> ValidateSessionAsync(Guid sessionId, string ipAddress, string userAgent);

    /// <summary>
    /// Updates session with new token hashes during refresh
    /// </summary>
    Task UpdateSessionTokensAsync(Guid sessionId, string newAccessTokenHash, string newRefreshTokenHash);

    /// <summary>
    /// Revokes (soft-deletes) a specific session
    /// </summary>
    Task RevokeSessionAsync(Guid sessionId);

    /// <summary>
    /// Revokes all sessions for a user except optionally one
    /// </summary>
    Task RevokeAllUserSessionsAsync(Guid userId, Guid? exceptSessionId = null);

    /// <summary>
    /// Retrieves active sessions for a user
    /// </summary>
    Task<List<SessionDto>> GetUserSessionsAsync(Guid userId);

    /// <summary>
    /// Gets session by refresh token hash
    /// </summary>
    Task<Session?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash);

    /// <summary>
    /// Updates last used timestamp for session
    /// </summary>
    Task UpdateSessionLastUsedAsync(Guid sessionId);
}
