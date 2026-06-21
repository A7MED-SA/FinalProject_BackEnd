using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Requested;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }

    public Payment Payment { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
}
