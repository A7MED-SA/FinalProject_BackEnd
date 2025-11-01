using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("payment_methods")]
public class PaymentMethod : BaseEntity
{

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("provider")]
    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(30)]
    public PaymentMethodType Type { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("configuration", TypeName = "nvarchar(max)")]
    public string? Configuration { get; set; }

    // Navigation Properties
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum PaymentMethodType
{
    CreditCard,
    DigitalWallet,
    BankTransfer
}
