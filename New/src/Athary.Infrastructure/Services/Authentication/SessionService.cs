using Athary.Application.DTOs.Auth;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Authentication;

public class SessionService : ISessionService
{
    private readonly IRepository<Session> _sessionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public SessionService(
        IRepository<Session> sessionRepo,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Session> CreateSessionAsync(
        User user,
        string refreshTokenHash,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var session = new Session
        {
            UserId = user.Id,
            RefreshTokenHash = refreshTokenHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _sessionRepo.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task<bool> ValidateSessionAsync(Guid sessionId, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive, cancellationToken: cancellationToken);

        if (session == null)
            return false;

        if (session.ExpiresAt < DateTime.UtcNow)
            return false;

        if (session.RefreshExpiresAt < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task UpdateSessionTokensAsync(Guid sessionId, string newRefreshTokenHash, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken: cancellationToken);

        if (session != null)
        {
            session.RefreshTokenHash = newRefreshTokenHash;
            session.LastActivityAt = DateTime.UtcNow;
            await _sessionRepo.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken: cancellationToken);

        if (session != null)
        {
            session.IsActive = false;
            await _sessionRepo.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, Guid? exceptSessionId = null, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepo.FindAsync(s => s.UserId == userId && s.IsActive, cancellationToken);

        foreach (var session in sessions)
        {
            if (exceptSessionId == null || session.Id != exceptSessionId.Value)
            {
                session.IsActive = false;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepo.FindAsync(s => s.UserId == userId && s.IsActive, cancellationToken);

        return sessions
            .OrderByDescending(s => s.LastActivityAt)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                IpAddress = s.IpAddress ?? "",
                UserAgent = s.UserAgent ?? "",
                CreatedAt = s.CreatedAt,
                LastUsed = s.LastActivityAt,
                IsActive = s.IsActive
            })
            .ToList();
    }

    public async Task<Session?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return await _sessionRepo.FirstOrDefaultAsync(
            s => s.RefreshTokenHash == refreshTokenHash && s.IsActive,
            q => q.Include(s => s.User),
            cancellationToken);
    }

    public async Task UpdateSessionLastUsedAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepo.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken: cancellationToken);

        if (session != null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await _sessionRepo.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
