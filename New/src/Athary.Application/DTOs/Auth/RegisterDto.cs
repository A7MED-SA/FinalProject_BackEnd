using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Auth;

public record RegisterDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public Gender? Gender { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? StreetLine1 { get; init; }
    public string? PostalCode { get; init; }
}
