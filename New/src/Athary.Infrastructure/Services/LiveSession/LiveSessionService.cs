using Athary.Application.DTOs.LiveSession;
using Athary.Application.Interfaces.LiveSession;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.LiveSession;

public sealed class LiveSessionService : ILiveSessionService
{
    private readonly ApplicationDbContext _context;
    private readonly IStreamingProvider _streamingProvider;

    public LiveSessionService(ApplicationDbContext context, IStreamingProvider streamingProvider)
    {
        _context = context;
        _streamingProvider = streamingProvider;
    }

    public async Task<LiveSessionResponseDto> CreateSessionAsync(CreateLiveSessionDto createDto, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId, cancellationToken);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        var room = await _streamingProvider.CreateRoomAsync(createDto.Title, createDto.MaxAttendees, cancellationToken);

        var session = new Athary.Domain.Entities.LiveSession
        {
            CourseId = section.CourseId,
            Title = createDto.Title,
            Description = createDto.Description,
            ScheduledStart = createDto.ScheduledStart,
            ScheduledEnd = createDto.ScheduledEnd,
            MeetingUrl = !string.IsNullOrEmpty(createDto.MeetingUrl) ? createDto.MeetingUrl : room.PlaybackUrl,
            Password = createDto.Password,
            MaxAttendees = createDto.MaxAttendees,
            Status = LiveSessionStatus.Scheduled
        };

        _context.LiveSessions.Add(session);

        var sectionItem = new SectionItem
        {
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.LiveSession,
            ItemId = session.Id,
            Position = await _context.SectionItems
                .Where(si => si.SectionId == createDto.SectionId)
                .CountAsync(cancellationToken) + 1,
            IsPreviewAllowed = false,
            IsMandatory = true
        };

        _context.SectionItems.Add(sectionItem);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(session);
    }

    public async Task<LiveSessionResponseDto> UpdateStatusAsync(Guid id, UpdateLiveSessionStatusDto statusDto, CancellationToken cancellationToken = default)
    {
        var session = await _context.LiveSessions
            .Include(s => s.LiveAttendances)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

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
            if (statusDto.Status == LiveSessionStatus.Finished)
            {
                await _streamingProvider.EndRoomAsync(session.MeetingUrl, cancellationToken);
            }
        }

        if (!string.IsNullOrEmpty(statusDto.MeetingUrl))
            session.MeetingUrl = statusDto.MeetingUrl;

        if (statusDto.Password != null)
            session.Password = statusDto.Password;

        if (statusDto.MaxAttendees.HasValue)
            session.MaxAttendees = statusDto.MaxAttendees;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(session);
    }

    public async Task<List<LiveSessionResponseDto>> GetSessionsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.LiveSessions
            .AsNoTracking()
            .Include(s => s.LiveAttendances)
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.ScheduledStart)
            .Select(s => MapToDto(s))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteSessionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _context.LiveSessions
            .Include(s => s.SectionItem)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (session == null)
            return false;

        if (session.Status == LiveSessionStatus.Live)
            throw new InvalidOperationException("Cannot delete a live session. End the session first.");

        if (session.SectionItem != null)
            _context.SectionItems.Remove(session.SectionItem);

        _context.LiveSessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static LiveSessionResponseDto MapToDto(Athary.Domain.Entities.LiveSession session)
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
