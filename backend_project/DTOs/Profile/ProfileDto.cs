using backend_project.Models;

namespace backend_project.DTOs.Profile;

/// <summary>
/// DTO لعرض بيانات الملف الشخصي الكاملة
/// </summary>
public class ProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Phone Numbers
    public List<PhoneDto> Phones { get; set; } = new();

    // Addresses
    public List<AddressDto> Addresses { get; set; } = new();
}

/// <summary>
/// DTO لعرض رقم الهاتف
/// </summary>
public class PhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public PhoneType Type { get; set; }
    public bool IsVerified { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// DTO لعرض العنوان
/// </summary>
public class AddressDto
{
    public Guid Id { get; set; }
    public AddressType Type { get; set; }
    public string StreetLine1 { get; set; } = string.Empty;
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateProvince { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsDefault { get; set; }
}

public class PublicProfileDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Bio { get; set; }
    public string? Nationality { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
