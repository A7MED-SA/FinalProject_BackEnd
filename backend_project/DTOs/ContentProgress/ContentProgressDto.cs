using System;
using backend_project.Models;

namespace backend_project.DTOs.ContentProgress;

public class ContentProgressDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public ContentType ContentType { get; set; }
    public Guid ContentId { get; set; }
    public bool IsCompleted { get; set; }
    public int WatchTimeSeconds { get; set; }
    public int AttemptsCount { get; set; }
    public decimal CompletionPercentage { get; set; }
    public string? Metadata { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
