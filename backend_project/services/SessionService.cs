using backend_project.Data;
using backend_project.DTOs.Auth;
using backend_project.Models;
using backend_project.Configuration;
using Microsoft.Extensions.Options;
using backend_project.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public SessionService(ApplicationDbContext context,IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Session> CreateSessionAsync(
    User user,
    string refreshTokenHash,
    string ipAddress,
    string userAgent)
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


    _context.Sessions.Add(session);
    await _context.SaveChangesAsync();

    return session;
    }

    public async Task<bool> ValidateSessionAsync(Guid sessionId, string ipAddress, string userAgent)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive);

        if (session == null)
            return false;

        // Validate IP and UserAgent for security
        // In production, you might want to make this configurable or less strict
        if (session.IpAddress != ipAddress)
            return false;

        if (session.UserAgent != userAgent)
            return false;

        return true;
    }

    public async Task UpdateSessionTokensAsync(Guid sessionId, string newRefreshTokenHash)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.RefreshTokenHash = newRefreshTokenHash;
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeSessionAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, Guid? exceptSessionId = null)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            if (exceptSessionId == null || session.Id != exceptSessionId.Value)
            {
                session.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<SessionDto>> GetUserSessionsAsync(Guid userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                CreatedAt = s.CreatedAt,
                LastUsed = s.LastActivityAt,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<Session?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash && s.IsActive);
    }

    public async Task UpdateSessionLastUsedAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
