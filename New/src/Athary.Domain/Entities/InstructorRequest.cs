using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class InstructorRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public InstructorRequestStatus Status { get; set; } = InstructorRequestStatus.Pending;
    public string? Message { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }

    public User User { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
    public List<InstructorRequestDocument> Documents { get; set; } = new List<InstructorRequestDocument>();
}
