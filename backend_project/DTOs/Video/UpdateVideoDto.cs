using System;
using backend_project.Models;

namespace backend_project.DTOs.Video;

public class UpdateVideoDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPreview { get; set; }
    public bool RequiresSubscription { get; set; }
    public string? Transcript { get; set; }
}
