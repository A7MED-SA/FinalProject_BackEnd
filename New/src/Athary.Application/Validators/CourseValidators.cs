using Athary.Application.DTOs.Courses;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateCourseDtoValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(5000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue);
    }
}

public sealed class AddRequirementDtoValidator : AbstractValidator<AddRequirementDto>
{
    public AddRequirementDtoValidator()
    {
        RuleFor(x => x.RequirementText)
            .NotEmpty()
            .MaximumLength(500);
    }
}

public sealed class AddLearningOutcomeDtoValidator : AbstractValidator<AddLearningOutcomeDto>
{
    public AddLearningOutcomeDtoValidator()
    {
        RuleFor(x => x.OutcomeText)
            .NotEmpty()
            .MaximumLength(500);
    }
}
