using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("live_sessions")]
public class LiveSession : BaseEntity
{
    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required, MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("scheduled_start")]
    public DateTime ScheduledStart { get; set; }

    [Column("scheduled_end")]
    public DateTime ScheduledEnd { get; set; }

    [Column("status")]
    public LiveSessionStatus Status { get; set; }

    [Column("meeting_url")]
    [MaxLength(2000)]
    public string MeetingUrl { get; set; } = string.Empty;

    [Column("password")]
    [MaxLength(255)]
    public string? Password { get; set; }

    [Column("max_attendees")]
    public int? MaxAttendees { get; set; }

    [Column("actual_start_at")]
    public DateTime? ActualStartAt { get; set; }

    [Column("actual_end_at")]
    public DateTime? ActualEndAt { get; set; }

    [Column("recording_file_id")]
    public Guid? RecordingFileId { get; set; }

    [ForeignKey(nameof(RecordingFileId))]
    public UploadedFile? RecordingFile { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    // Navigation to SectionItem (if session is part of a course section)
    public virtual SectionItem? SectionItem { get; set; }

    public virtual ICollection<LiveAttendance> LiveAttendances { get; set; } = new List<LiveAttendance>();
}

public enum LiveSessionStatus
{
    Scheduled,
    Live,
    Finished,
    Cancelled
}
