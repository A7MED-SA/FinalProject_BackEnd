using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("payments")]
public class Payment : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Required]
    [Column("payment_method_id")]
    public Guid PaymentMethodId { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column("currency")]
    [MaxLength(3)]
    public PaymentCurrency Currency { get; set; } = PaymentCurrency.EGP;

    [Column("status")]
    [MaxLength(20)]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Required]
    [Column("transaction_ref")]
    [MaxLength(255)]
    public string TransactionRef { get; set; } = string.Empty;

    [Column("gateway_response")]
    [MaxLength(2000)]
    public string? GatewayResponse { get; set; }

    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual ICollection<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

public enum PaymentCurrency
{
    EGP,
    USD,
    EUR
}

public enum PaymentStatus
{
    Succeeded,
    Pending,
    Failed
}
