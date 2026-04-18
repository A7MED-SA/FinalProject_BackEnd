using System.ComponentModel.DataAnnotations;
using backend_project.Models;

namespace backend_project.DTOs.Profile;

/// <summary>
/// DTO لتحديث بيانات الملف الشخصي الأساسية
/// </summary>
public class UpdateProfileDto
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public Gender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }
}

/// <summary>
/// DTO لإضافة رقم هاتف جديد
/// </summary>
public class AddPhoneDto
{
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public PhoneType Type { get; set; } = PhoneType.Primary;

    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// DTO لإضافة عنوان جديد
/// </summary>
public class AddAddressDto
{
    public AddressType Type { get; set; } = AddressType.Home;

    [Required]
    [MaxLength(255)]
    public string StreetLine1 { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? StreetLine2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? StateProvince { get; set; }

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public bool IsDefault { get; set; } = false;
}

public class SetProfileImageDto
{
    public Guid FileId { get; set; }
}

/// <summary>
/// DTO لتحديث عنوان موجود
/// </summary>
public class UpdateAddressDto
{
    public AddressType? Type { get; set; }

    [MaxLength(255)]
    public string? StreetLine1 { get; set; }

    [MaxLength(255)]
    public string? StreetLine2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? StateProvince { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public bool? IsDefault { get; set; }
}
