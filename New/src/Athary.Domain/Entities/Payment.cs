using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public PaymentCurrency Currency { get; set; } = PaymentCurrency.EGP;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string TransactionRef { get; set; } = string.Empty;
    public string? GatewayResponse { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
    public List<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
    public List<Refund> Refunds { get; set; } = new List<Refund>();
}
