using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public ReportEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public string? AdminNote { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }

    public User Reporter { get; set; } = null!;
    public User? Resolver { get; set; }
}
