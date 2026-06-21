using Athary.Application.DTOs.Auth;
using Athary.Domain.Entities;

namespace Athary.Application.Interfaces.Authentication;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(
        User user,
        string refreshTokenHash,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateSessionAsync(Guid sessionId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task UpdateSessionTokensAsync(Guid sessionId, string newRefreshTokenHash, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task RevokeAllUserSessionsAsync(Guid userId, Guid? exceptSessionId = null, CancellationToken cancellationToken = default);
    Task<List<SessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Session?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task UpdateSessionLastUsedAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
