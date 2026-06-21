namespace Athary.Domain.Entities;

public sealed class CommentLike : BaseEntity
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VideoComment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}
