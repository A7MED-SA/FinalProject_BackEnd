using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public PaymentMethodType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Configuration { get; set; }

    public List<Payment> Payments { get; set; } = new List<Payment>();
}
