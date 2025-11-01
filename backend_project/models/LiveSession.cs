using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("live_sessions")]
public class LiveSession : BaseEntity
{

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("scheduled_start")]
    public DateTime ScheduledStart { get; set; }

    [Column("scheduled_end")]
    public DateTime ScheduledEnd { get; set; }

    [Column("actual_start")]
    public DateTime? ActualStart { get; set; }

    [Column("actual_end")]
    public DateTime? ActualEnd { get; set; }

    [Column("meeting_url")]
    [MaxLength(500)]
    public string? MeetingUrl { get; set; }

    [Column("meeting_password")]
    [MaxLength(100)]
    public string? MeetingPassword { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public LiveSessionStatus Status { get; set; } = LiveSessionStatus.Scheduled;

    [Column("max_attendees")]
    public Guid? MaxAttendees { get; set; }

    [Column("is_recorded")]
    public bool IsRecorded { get; set; } = false;

    [Column("recording_url")]
    [MaxLength(500)]
    public string? RecordingUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    public virtual SectionItem? SectionItem { get; set; }
    public virtual ICollection<LiveAttendance> LiveAttendances { get; set; } = new List<LiveAttendance>();
}

public enum LiveSessionStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled
}
