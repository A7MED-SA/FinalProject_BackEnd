using System;
using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.Video;

public class CreateVideoDto
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public VideoProvider Provider { get; set; } = VideoProvider.Local;
    public string? ExternalId { get; set; }
    public Guid VideoFileId { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsPreview { get; set; } = false;
    public bool RequiresSubscription { get; set; } = true;
    public string? Transcript { get; set; }
}
