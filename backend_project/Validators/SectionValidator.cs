using backend_project.DTOs.Section;
using FluentValidation;

namespace backend_project.Validators;

public class CreateSectionValidator : AbstractValidator<CreateSectionDto>
{
    public CreateSectionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");
    }
}

public class UpdateSectionValidator : AbstractValidator<UpdateSectionDto>
{
    public UpdateSectionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");
    }
}

public class CreateSectionItemValidator : AbstractValidator<CreateSectionItemDto>
{
    public CreateSectionItemValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
            
        RuleFor(x => x.ItemType)
            .IsInEnum().WithMessage("Invalid Item Type.");
    }
}
