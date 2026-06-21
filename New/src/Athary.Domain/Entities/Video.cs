using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Video : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid VideoFileId { get; set; }
    public Guid? ThumbnailFileId { get; set; }
    public VideoProvider Provider { get; set; }
    public string? ProviderVideoId { get; set; }
    public VideoQuality Quality { get; set; } = VideoQuality._720p;
    public int DurationSeconds { get; set; }
    public string? Transcript { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Processing;
    public int ViewCount { get; set; }

    public UploadedFile VideoFile { get; set; } = null!;
    public UploadedFile? ThumbnailFile { get; set; }
    public SectionItem? SectionItem { get; set; }
    public List<VideoComment> VideoComments { get; set; } = new List<VideoComment>();
}
