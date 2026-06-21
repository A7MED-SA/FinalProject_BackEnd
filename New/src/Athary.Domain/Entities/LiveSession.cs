using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class LiveSession : BaseEntity
{
    public Guid CourseId { get; set; }
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

    public UploadedFile? RecordingFile { get; set; }
    public Course Course { get; set; } = null!;
    public SectionItem? SectionItem { get; set; }
    public List<LiveAttendance> LiveAttendances { get; set; } = new List<LiveAttendance>();
}
