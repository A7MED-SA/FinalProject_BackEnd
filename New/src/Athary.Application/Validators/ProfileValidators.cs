using Athary.Application.DTOs.Profile;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).When(x => x.LastName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(1000).When(x => x.Bio is not null);

        RuleFor(x => x.Gender)
            .IsInEnum().When(x => x.Gender is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth is not null)
            .WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.Nationality)
            .MaximumLength(100).When(x => x.Nationality is not null);
    }
}

public sealed class AddPhoneDtoValidator : AbstractValidator<AddPhoneDto>
{
    public AddPhoneDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[\d\s\-()]{7,20}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

public sealed class AddAddressDtoValidator : AbstractValidator<AddAddressDto>
{
    public AddAddressDtoValidator()
    {
        RuleFor(x => x.StreetLine1)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.StreetLine2)
            .MaximumLength(255).When(x => x.StreetLine2 is not null);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.StateProvince)
            .MaximumLength(100).When(x => x.StateProvince is not null);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ContactPhone)
            .Matches(@"^\+?[\d\s\-()]{7,20}$")
            .When(x => x.ContactPhone is not null)
            .WithMessage("Invalid phone number format");
    }
}

public sealed class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
{
    public UpdateAddressDtoValidator()
    {
        RuleFor(x => x.StreetLine1)
            .MaximumLength(255).When(x => x.StreetLine1 is not null);

        RuleFor(x => x.StreetLine2)
            .MaximumLength(255).When(x => x.StreetLine2 is not null);

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City is not null);

        RuleFor(x => x.StateProvince)
            .MaximumLength(100).When(x => x.StateProvince is not null);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).When(x => x.PostalCode is not null);

        RuleFor(x => x.Country)
            .MaximumLength(100).When(x => x.Country is not null);

        RuleFor(x => x.ContactPhone)
            .Matches(@"^\+?[\d\s\-()]{7,20}$")
            .When(x => x.ContactPhone is not null)
            .WithMessage("Invalid phone number format");
    }
}
