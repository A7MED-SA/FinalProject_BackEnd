using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("announcements")]
public class Announcement : BaseEntity
{

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("target")]
    [MaxLength(20)]
    public AnnouncementTarget Target { get; set; } = AnnouncementTarget.All;

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("course_id")]
    public Guid? CourseId { get; set; }

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Navigation Properties
    [ForeignKey("CreatedBy")]
    public virtual User Creator { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course? Course { get; set; }
}

public enum AnnouncementTarget
{
    All,
    Students,
    Teachers,
    Admins,
    SpecificCourse
}
