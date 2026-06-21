using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record CreateVideoDto
{
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid VideoFileId { get; init; }
    public VideoProvider Provider { get; init; } = VideoProvider.Local;
    public string? ProviderVideoId { get; init; }
    public int DurationSeconds { get; init; }
    public string? Transcript { get; init; }
    public bool IsPreview { get; init; }
}

public sealed record UpdateVideoDto
{
    public string Title { get; init; } = string.Empty;
    public string? Transcript { get; init; }
    public bool IsPreview { get; init; }
}

public sealed record VideoResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public VideoProvider Provider { get; init; }
    public int DurationSeconds { get; init; }
    public VideoQuality Quality { get; init; }
    public VideoStatus Status { get; init; }
    public bool IsPreview { get; init; }
    public int ViewCount { get; init; }
    public string? Transcript { get; init; }
    public DateTime CreatedAt { get; init; }
}
