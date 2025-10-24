using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("cart_items")]
public class CartItem : BaseEntity
{

    [Required]
    [Column("cart_id")]
    public Guid CartId { get; set; }

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Column("price_snapshot", TypeName = "decimal(18,2)")]
    public decimal PriceSnapshot { get; set; }

    [Column("added_at")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CartId")]
    public virtual Cart Cart { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
}
