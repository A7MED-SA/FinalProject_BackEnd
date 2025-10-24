using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("orders")]
public class Order : BaseEntity
{

    [Required]
    [Column("order_number")]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("billing_address_id")]
    public Guid? BillingAddressId { get; set; }

    [Required]
    [Column("subtotal_amount", TypeName = "decimal(18,2)")]
    public decimal SubtotalAmount { get; set; }

    [Column("coupon_id")]
    public Guid? CouponId { get; set; }

    [Column("discount_amount", TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column("tax_amount", TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    [Required]
    [Column("final_amount", TypeName = "decimal(18,2)")]
    public decimal FinalAmount { get; set; }

    [Column("currency")]
    [MaxLength(3)]
    public string Currency { get; set; } = "EGP";

    [Column("status")]
    [MaxLength(20)]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("BillingAddressId")]
    public virtual Address? BillingAddress { get; set; }

    [ForeignKey("CouponId")]
    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

public enum OrderStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}
