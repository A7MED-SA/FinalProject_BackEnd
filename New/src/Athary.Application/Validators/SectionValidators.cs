using Athary.Application.DTOs.Courses;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateSectionDtoValidator : AbstractValidator<CreateSectionDto>
{
    public CreateSectionDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}

public sealed class UpdateSectionDtoValidator : AbstractValidator<UpdateSectionDto>
{
    public UpdateSectionDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}

public sealed class CreateSectionItemDtoValidator : AbstractValidator<CreateSectionItemDto>
{
    public CreateSectionItemDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty();
    }
}
