using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("sessions")]
public class Session : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("token_hash")]
    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    [Column("refresh_token_hash")]
    [MaxLength(255)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    [Column("ip_address")]
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("refresh_expires_at")]
    public DateTime RefreshExpiresAt { get; set; }

    [Column("last_activity_at")]
    public DateTime? LastActivityAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
