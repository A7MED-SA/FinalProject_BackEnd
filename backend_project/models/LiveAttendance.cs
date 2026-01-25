using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("live_attendances")]
public class LiveAttendance : BaseEntity
{

    [Required]
    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [Column("left_at")]
    public DateTime? LeftAt { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    // Navigation Properties
    [ForeignKey("SessionId")]
    public virtual LiveSession Session { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
