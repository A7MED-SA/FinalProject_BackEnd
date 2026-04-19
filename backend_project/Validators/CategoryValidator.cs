using backend_project.DTOs.Category;
using FluentValidation;

namespace backend_project.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
            
        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be a positive number.");
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
            
        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be a positive number.");
    }
}
