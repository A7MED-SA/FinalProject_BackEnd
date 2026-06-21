using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Profile;

public sealed record UpdateProfileDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Bio { get; init; }
    public Gender? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Nationality { get; init; }
}

public sealed record AddPhoneDto
{
    public string PhoneNumber { get; init; } = string.Empty;
    public PhoneType Type { get; init; } = PhoneType.Primary;
    public bool IsDefault { get; init; }
}

public sealed record AddAddressDto
{
    public string Type { get; init; } = "Home";
    public string StreetLine1 { get; init; } = string.Empty;
    public string? StreetLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? StateProvince { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? ContactPhone { get; init; }
    public bool IsDefault { get; init; }
}

public sealed record UpdateAddressDto
{
    public string? Type { get; init; }
    public string? StreetLine1 { get; init; }
    public string? StreetLine2 { get; init; }
    public string? City { get; init; }
    public string? StateProvince { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? ContactPhone { get; init; }
    public bool? IsDefault { get; init; }
}

public sealed record SetProfileImageDto
{
    public Guid FileId { get; init; }
}
