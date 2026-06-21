using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public decimal SubtotalAmount { get; set; }
    public Guid? CouponId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Address? BillingAddress { get; set; }
    public Coupon? Coupon { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<Payment> Payments { get; set; } = new List<Payment>();
    public List<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}
