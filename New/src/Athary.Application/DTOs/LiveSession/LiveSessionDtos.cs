using Athary.Domain.Enums;

namespace Athary.Application.DTOs.LiveSession;

public class CreateLiveSessionDto
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string MeetingUrl { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int? MaxAttendees { get; set; }
}

public class UpdateLiveSessionStatusDto
{
    public LiveSessionStatus Status { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Password { get; set; }
    public int? MaxAttendees { get; set; }
}

public class LiveSessionResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public LiveSessionStatus Status { get; set; }
    public string MeetingUrl { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int? MaxAttendees { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? ActualEndAt { get; set; }
    public Guid? RecordingFileId { get; set; }
    public int CurrentAttendeesCount { get; set; }
}
