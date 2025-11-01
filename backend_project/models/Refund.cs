using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("refunds")]
public class Refund : BaseEntity
{

    [Required]
    [Column("payment_id")]
    public Guid PaymentId { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public RefundStatus Status { get; set; } = RefundStatus.Requested;

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Column("processed_by")]
    public Guid? ProcessedBy { get; set; }

    // Navigation Properties
    [ForeignKey("PaymentId")]
    public virtual Payment Payment { get; set; } = null!;

    [ForeignKey("ProcessedBy")]
    public virtual User? ProcessedByUser { get; set; }
}

public enum RefundStatus
{
    Requested,
    Approved,
    Rejected,
    Processed
}
