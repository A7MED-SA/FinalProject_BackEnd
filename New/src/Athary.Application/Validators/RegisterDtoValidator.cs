using Athary.Application.DTOs.Auth;
using FluentValidation;

namespace Athary.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-()]{7,20}$").When(x => x.PhoneNumber is not null)
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Gender)
            .IsInEnum().When(x => x.Gender is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth is not null)
            .WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.Country)
            .MaximumLength(100).When(x => x.Country is not null);

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).When(x => x.PostalCode is not null);
    }
}
