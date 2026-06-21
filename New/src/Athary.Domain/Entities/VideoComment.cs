namespace Athary.Domain.Entities;

public sealed class VideoComment : BaseEntity
{
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
    public VideoComment? ParentComment { get; set; }
    public List<VideoComment> Replies { get; set; } = new List<VideoComment>();
    public List<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
