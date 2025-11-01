using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("transaction_logs")]
public class TransactionLog : BaseEntity
{

    [Required]
    [Column("payment_id")]
    public Guid PaymentId { get; set; }

    [Required]
    [Column("action")]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [Column("details")]
    public string? Details { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("PaymentId")]
    public virtual Payment Payment { get; set; } = null!;
}
