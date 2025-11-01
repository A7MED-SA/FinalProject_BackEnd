using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("coupon_courses")]
public class CouponCourse : BaseEntity
{

    [Required]
    [Column("coupon_id")]
    public Guid CouponId { get; set; }

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    // Navigation Properties
    [ForeignKey("CouponId")]
    public virtual Coupon Coupon { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
}
