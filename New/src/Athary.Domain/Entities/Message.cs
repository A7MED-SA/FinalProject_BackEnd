namespace Athary.Domain.Entities;

public sealed class Message : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }

    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
