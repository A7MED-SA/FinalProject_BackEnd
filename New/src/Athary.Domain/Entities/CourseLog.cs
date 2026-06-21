using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class CourseLog : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public CourseLogAction Action { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Course Course { get; set; } = null!;
    public User User { get; set; } = null!;
}
