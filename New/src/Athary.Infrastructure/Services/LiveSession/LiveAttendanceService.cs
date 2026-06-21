using Athary.Application.Interfaces.LiveSession;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.LiveSession;

public sealed class LiveAttendanceService : ILiveAttendanceService
{
    private readonly ApplicationDbContext _context;

    public LiveAttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> JoinSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _context.LiveSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
            throw new KeyNotFoundException("Live session not found.");

        if (session.Status != LiveSessionStatus.Live)
            throw new InvalidOperationException("Session is not currently live.");

        var activeAttendees = await _context.LiveAttendances
            .CountAsync(a => a.SessionId == sessionId && a.LeftAt == null, cancellationToken);

        if (session.MaxAttendees.HasValue && activeAttendees >= session.MaxAttendees.Value)
            throw new InvalidOperationException("Maximum attendee limit reached for this session.");

        var existing = await _context.LiveAttendances
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId && a.LeftAt == null, cancellationToken);

        if (existing != null)
            return true;

        var attendance = new LiveAttendance
        {
            SessionId = sessionId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.LiveAttendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> LeaveSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var attendance = await _context.LiveAttendances
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId && a.LeftAt == null, cancellationToken);

        if (attendance == null)
            throw new KeyNotFoundException("Attendance record not found.");

        attendance.LeftAt = DateTime.UtcNow;
        attendance.DurationMinutes = (int)(DateTime.UtcNow - attendance.JoinedAt).TotalMinutes;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> GetAttendanceCountAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.LiveAttendances
            .CountAsync(a => a.SessionId == sessionId, cancellationToken);
    }
}
