using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementTarget Target { get; set; } = AnnouncementTarget.All;
    public Guid CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CourseId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public User Creator { get; set; } = null!;
    public Course? Course { get; set; }
}
