namespace Athary.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid CourseId { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
