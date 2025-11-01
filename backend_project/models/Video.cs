using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("videos")]
public class Video : BaseEntity
{

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("url")]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [Column("thumbnail_url")]
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [Column("provider")]
    [MaxLength(30)]
    public VideoProvider Provider { get; set; }

    [Column("provider_video_id")]
    [MaxLength(255)]
    public string? ProviderVideoId { get; set; }

    [Column("quality")]
    [MaxLength(10)]
    public VideoQuality Quality { get; set; } = VideoQuality._720p;

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; } = 0;

    [Column("transcript")]
    public string? Transcript { get; set; }

    [Column("has_subtitles")]
    public bool HasSubtitles { get; set; } = false;

    [Column("skip_intro_seconds")]
    public int? SkipIntroSeconds { get; set; }

    [Column("skip_outro_seconds")]
    public int? SkipOutroSeconds { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public VideoStatus Status { get; set; } = VideoStatus.Processing;

    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual SectionItem? SectionItem { get; set; }
    public virtual ICollection<VideoComment> VideoComments { get; set; } = new List<VideoComment>();
}

public enum VideoProvider
{
    Youtube,
    Vimeo,
    CloudflareStream
}

public enum VideoQuality
{
    _720p,
    _1080p,
    _4k
}

public enum VideoStatus
{
    Processing,
    Ready,
    Failed
}
