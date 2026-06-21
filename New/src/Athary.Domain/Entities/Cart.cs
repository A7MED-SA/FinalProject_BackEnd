namespace Athary.Domain.Entities;

public sealed class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; } = null!;
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
