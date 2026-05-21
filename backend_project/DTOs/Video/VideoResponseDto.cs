using System;
using backend_project.Models;

namespace backend_project.DTOs.Video;

public class VideoResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public VideoProvider Provider { get; set; }
    public int DurationSeconds { get; set; }
    public VideoQuality Quality { get; set; }
    public VideoStatus Status { get; set; }
    public bool IsPreview { get; set; }
    public bool RequiresSubscription { get; set; }
    public int ViewCount { get; set; }
    public string? Transcript { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
