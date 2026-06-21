namespace Athary.Domain.Entities;

public sealed class CouponCourse : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid CourseId { get; set; }

    public Coupon Coupon { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
