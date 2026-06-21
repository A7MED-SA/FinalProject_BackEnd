namespace Athary.Application.DTOs.Courses;

public sealed record CreateDocumentDto
{
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid FileId { get; init; }
    public bool IsDownloadable { get; init; } = true;
}

public sealed record UpdateDocumentDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDownloadable { get; init; } = true;
}

public sealed record DocumentResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? FileUrl { get; init; }
    public string? FileType { get; init; }
    public long? FileSizeBytes { get; init; }
    public int DownloadCount { get; init; }
    public bool IsDownloadable { get; init; }
    public DateTime CreatedAt { get; init; }
}
