using FluentValidation;
using backend_project.DTOs.Quiz;

namespace backend_project.Validators;

public class CreateQuizValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.PassingScorePercent)
            .InclusiveBetween(0, 100).WithMessage("PassingScorePercent must be between 0 and 100.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).When(x => x.DurationMinutes.HasValue)
            .WithMessage("DurationMinutes must be greater than 0.");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).When(x => x.MaxAttempts.HasValue)
            .WithMessage("MaxAttempts must be greater than 0.");
    }
}
