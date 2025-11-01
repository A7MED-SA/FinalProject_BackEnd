using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("video_comments")]
public class VideoComment : BaseEntity
{

    [Required]
    [Column("video_id")]
    public Guid VideoId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("parent_comment_id")]
    public Guid? ParentCommentId { get; set; }

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("is_edited")]
    public bool IsEdited { get; set; } = false;

    [Column("likes_count")]
    public int LikesCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ParentCommentId")]
    public virtual VideoComment? ParentComment { get; set; }

    public virtual ICollection<VideoComment> Replies { get; set; } = new List<VideoComment>();
    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
