using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("course_logs")]
public class CourseLog : BaseEntity
{

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("action")]
    [MaxLength(20)]
    public CourseLogAction Action { get; set; }

    [Column("details")]
    public string? Details { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

public enum CourseLogAction
{
    Created,
    Updated,
    Published,
    Archived
}
