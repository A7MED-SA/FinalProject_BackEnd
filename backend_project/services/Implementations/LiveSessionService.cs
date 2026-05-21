using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.LiveSession;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class LiveSessionService : ILiveSessionService
{
    private readonly ApplicationDbContext _context;

    public LiveSessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LiveSessionResponseDto> CreateSessionAsync(CreateLiveSessionDto createDto)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        var session = new LiveSession
        {
            Id = Guid.NewGuid(),
            CourseId = section.CourseId,
            Title = createDto.Title,
            Description = createDto.Description,
            ScheduledStart = createDto.ScheduledStart,
            ScheduledEnd = createDto.ScheduledEnd,
            MeetingUrl = createDto.MeetingUrl,
            Password = createDto.Password,
            MaxAttendees = createDto.MaxAttendees,
            Status = LiveSessionStatus.Scheduled
        };

        _context.LiveSessions.Add(session);

        var sectionItem = new SectionItem
        {
            Id = Guid.NewGuid(),
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.LiveSession,
            ItemId = session.Id,
            Position = await _context.SectionItems
                .Where(si => si.SectionId == createDto.SectionId)
                .CountAsync() + 1,
            IsPreviewAllowed = false,
            IsMandatory = true
        };

        _context.SectionItems.Add(sectionItem);
        await _context.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<LiveSessionResponseDto> UpdateStatusAsync(Guid id, UpdateLiveSessionStatusDto statusDto)
    {
        var session = await _context.LiveSessions
            .Include(s => s.LiveAttendances)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            throw new KeyNotFoundException("Live session not found.");

        session.Status = statusDto.Status;

        if (statusDto.Status == LiveSessionStatus.Live)
        {
            session.ActualStartAt = DateTime.UtcNow;
        }
        else if (statusDto.Status == LiveSessionStatus.Finished || statusDto.Status == LiveSessionStatus.Cancelled)
        {
            session.ActualEndAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<IEnumerable<LiveSessionResponseDto>> GetSessionsForCourseAsync(Guid courseId)
    {
        return await _context.LiveSessions
            .AsNoTracking()
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.ScheduledStart)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<bool> DeleteSessionAsync(Guid id)
    {
        var session = await _context.LiveSessions
            .Include(s => s.SectionItem)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return false;

        if (session.Status == LiveSessionStatus.Live)
            throw new InvalidOperationException("Cannot delete a live session. End the session first.");

        if (session.SectionItem != null)
            _context.SectionItems.Remove(session.SectionItem);

        _context.LiveSessions.Remove(session);
        await _context.SaveChangesAsync();

        return true;
    }

    private static LiveSessionResponseDto MapToDto(LiveSession session)
    {
        return new LiveSessionResponseDto
        {
            Id = session.Id,
            Title = session.Title,
            Description = session.Description,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            Status = session.Status,
            MeetingUrl = session.MeetingUrl,
            Password = session.Password,
            MaxAttendees = session.MaxAttendees,
            ActualStartAt = session.ActualStartAt,
            ActualEndAt = session.ActualEndAt,
            RecordingFileId = session.RecordingFileId,
            CurrentAttendeesCount = session.LiveAttendances?.Count(a => a.LeftAt == null) ?? 0
        };
    }
}
