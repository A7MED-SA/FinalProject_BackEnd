using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("videos")]
public class Video : BaseEntity
{
    [Required, MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("video_file_id")]
    public Guid VideoFileId { get; set; }

    [Column("thumbnail_file_id")]
    public Guid? ThumbnailFileId { get; set; }

    [Column("provider")]
    public VideoProvider Provider { get; set; }

    [Column("provider_video_id")]
    [MaxLength(255)]
    public string? ProviderVideoId { get; set; }

    [Column("quality")]
    public VideoQuality Quality { get; set; } = VideoQuality._720p;

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("transcript")]
    public string? Transcript { get; set; }

    [Column("status")]
    public VideoStatus Status { get; set; } = VideoStatus.Processing;

    [Column("view_count")]
    public int ViewCount { get; set; }

    [ForeignKey(nameof(VideoFileId))]
    public UploadedFile VideoFile { get; set; } = null!;

    [ForeignKey(nameof(ThumbnailFileId))]
    public UploadedFile? ThumbnailFile { get; set; }

    public virtual ICollection<VideoComment> VideoComments { get; set; } = new List<VideoComment>();

    // Navigation to SectionItem (if video is part of a course section)
    public virtual SectionItem? SectionItem { get; set; }
}

public enum VideoProvider
{
    Local,
    YouTube,
    Vimeo,
    Minio
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
