namespace Athary.Domain.Entities;

public sealed class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid FileId { get; set; }
    public int DownloadCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UploadedFile File { get; set; } = null!;
    public SectionItem? SectionItem { get; set; }
}
