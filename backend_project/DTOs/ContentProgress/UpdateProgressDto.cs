using System;

namespace backend_project.DTOs.ContentProgress;

public class UpdateProgressDto
{
    public int WatchTimeSeconds { get; set; } = 0;
    public decimal? CompletionPercentage { get; set; }
    public string? Metadata { get; set; }
    public bool MarkAsCompleted { get; set; } = false;
}
