using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("notifications")]
public class Notification : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("type")]
    [MaxLength(20)]
    public NotificationType Type { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    public string? Message { get; set; }

    [Column("link_url")]
    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

public enum NotificationType
{
    Course,
    Payment,
    System,
    Message
}
