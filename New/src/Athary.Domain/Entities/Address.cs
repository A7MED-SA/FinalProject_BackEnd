namespace Athary.Domain.Entities;

public sealed class Address : BaseEntity
{
    public Guid UserId { get; set; }

    public string Type { get; set; } = "Home";

    public string StreetLine1 { get; set; } = string.Empty;

    public string? StreetLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string? StateProvince { get; set; }

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? ContactPhone { get; set; }

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
