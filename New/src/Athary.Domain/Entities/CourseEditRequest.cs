using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class CourseEditRequest : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid RequestedBy { get; set; }
    public EditRequestType RequestType { get; set; }
    public Guid? TargetSectionId { get; set; }
    public Guid? TargetItemId { get; set; }
    public EditOperation Operation { get; set; }
    public string? JsonPayload { get; set; }
    public EditRequestStatus Status { get; set; } = EditRequestStatus.Pending;
    public string? AdminNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public bool IsEmergency { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public Course Course { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
