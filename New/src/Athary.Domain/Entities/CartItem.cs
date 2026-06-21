namespace Athary.Domain.Entities;

public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid CourseId { get; set; }
    public decimal PriceSnapshot { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Cart Cart { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
