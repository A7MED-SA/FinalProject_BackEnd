using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("addresses")]
public class Address : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("type")]
    [MaxLength(20)]
    public AddressType Type { get; set; } = AddressType.Home;

    [Required]
    [Column("street_line_1")]
    [MaxLength(255)]
    public string StreetLine1 { get; set; } = string.Empty;

    [Column("street_line_2")]
    [MaxLength(255)]
    public string? StreetLine2 { get; set; }

    [Required]
    [Column("city")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Column("state_province")]
    [MaxLength(100)]
    public string? StateProvince { get; set; }

    [Required]
    [Column("postal_code")]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [Column("country")]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Column("contact_phone")]
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

public enum AddressType
{
    Billing,
    Home,
    Work
}
