using System.ComponentModel.DataAnnotations;

namespace Athary.Domain.Entities;

public class Testimonial : BaseEntity
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsApproved { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsFlagged { get; set; } = false;

    [StringLength(500)]
    public string? FlagReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
