using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsFlagged { get; set; }
    public Guid? FlaggedBy { get; set; }
    public DateTime? FlaggedAt { get; set; }

    public User? FlaggedByUser { get; set; }
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public User? Moderator { get; set; }
    public List<ReviewHelpful> ReviewHelpfuls { get; set; } = new List<ReviewHelpful>();
}
