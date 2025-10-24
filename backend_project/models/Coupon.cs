using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("coupons")]
public class Coupon : BaseEntity
{

    [Required]
    [Column("code")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(20)]
    public CouponType Type { get; set; } = CouponType.Percentage;

    [Required]
    [Column("value", TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    [Column("max_discount_amount", TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column("minimum_purchase_amount", TypeName = "decimal(18,2)")]
    public decimal? MinimumPurchaseAmount { get; set; }

    [Column("applicable_to")]
    [MaxLength(20)]
    public CouponApplicableTo ApplicableTo { get; set; } = CouponApplicableTo.All;

    [Column("usage_limit")]
    public int? UsageLimit { get; set; }

    [Column("user_limit_per_user")]
    public int? UserLimitPerUser { get; set; }

    [Column("times_used")]
    public int TimesUsed { get; set; } = 0;

    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    [Column("valid_from")]
    public DateTime? ValidFrom { get; set; }

    [Column("valid_until")]
    public DateTime? ValidUntil { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CreatedBy")]
    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<CouponCourse> CouponCourses { get; set; } = new List<CouponCourse>();
    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum CouponType
{
    Percentage,
    Fixed
}

public enum CouponApplicableTo
{
    All,
    SpecificCourses,
    Category
}
