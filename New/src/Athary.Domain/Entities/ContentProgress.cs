using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class ContentProgress : BaseEntity
{
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

    public Enrollment Enrollment { get; set; } = null!;
}
