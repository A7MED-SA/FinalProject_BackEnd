using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Profile;

public sealed record ProfileDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public Gender? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Nationality { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PhoneDto> Phones { get; init; } = new();
    public List<AddressDto> Addresses { get; init; } = new();
}

public sealed record PublicProfileDto
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string? Slug { get; init; }
    public string? Bio { get; init; }
    public string? Nationality { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record PhoneDto
{
    public Guid Id { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public PhoneType Type { get; init; }
    public bool IsVerified { get; init; }
    public bool IsDefault { get; init; }
}

public sealed record AddressDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string StreetLine1 { get; init; } = string.Empty;
    public string? StreetLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string? StateProvince { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? ContactPhone { get; init; }
    public bool IsDefault { get; init; }
}
