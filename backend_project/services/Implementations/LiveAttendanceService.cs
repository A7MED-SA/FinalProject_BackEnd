using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class LiveAttendanceService : ILiveAttendanceService
{
    private readonly ApplicationDbContext _context;

    public LiveAttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> JoinSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await _context.LiveSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new KeyNotFoundException("Live session not found.");

        if (session.Status != LiveSessionStatus.Live)
            throw new InvalidOperationException("Session is not currently live.");

        var activeAttendees = await _context.LiveAttendances
            .CountAsync(a => a.SessionId == sessionId && a.LeftAt == null);

        if (session.MaxAttendees.HasValue && activeAttendees >= session.MaxAttendees.Value)
            throw new InvalidOperationException("Maximum attendee limit reached for this session.");

        var existing = await _context.LiveAttendances
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId && a.LeftAt == null);

        if (existing != null)
            return true;

        var attendance = new LiveAttendance
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.LiveAttendances.Add(attendance);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeaveSessionAsync(Guid sessionId, Guid userId)
    {
        var attendance = await _context.LiveAttendances
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId && a.LeftAt == null);

        if (attendance == null)
            throw new KeyNotFoundException("Attendance record not found.");

        attendance.LeftAt = DateTime.UtcNow;
        attendance.DurationMinutes = (int)(DateTime.UtcNow - attendance.JoinedAt).TotalMinutes;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetAttendanceCountAsync(Guid sessionId)
    {
        return await _context.LiveAttendances
            .CountAsync(a => a.SessionId == sessionId);
    }
}
