using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; } = CouponType.Percentage;
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public CouponApplicableTo ApplicableTo { get; set; } = CouponApplicableTo.All;
    public int? UsageLimit { get; set; }
    public int? UserLimitPerUser { get; set; }
    public int TimesUsed { get; set; }
    public bool IsPublic { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Creator { get; set; } = null!;
    public List<CouponCourse> CouponCourses { get; set; } = new List<CouponCourse>();
    public List<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public List<Order> Orders { get; set; } = new List<Order>();
}
