using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("coupon_usages")]
public class CouponUsage : BaseEntity
{

    [Required]
    [Column("coupon_id")]
    public Guid CouponId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("used_at")]
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CouponId")]
    public virtual Coupon Coupon { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
}
