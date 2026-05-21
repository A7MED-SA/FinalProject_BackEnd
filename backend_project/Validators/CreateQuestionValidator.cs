using FluentValidation;
using backend_project.DTOs.Quiz;

namespace backend_project.Validators;

public class CreateQuestionValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("QuestionText is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid question type.");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Points must be greater than 0.");

        RuleFor(x => x.Options)
            .Must((dto, options) =>
            {
                if (dto.Type == Models.QuestionType.TrueFalse)
                    return options.Count == 2;
                if (dto.Type == Models.QuestionType.MultipleChoice)
                    return options.Count >= 2;
                return true;
            }).WithMessage("At least 2 options are required for MultipleChoice; exactly 2 for TrueFalse.");

        RuleFor(x => x.Options)
            .Must(options => options.Count(o => o.IsCorrect) == 1)
            .When(x => x.Type != Models.QuestionType.ShortAnswer)
            .WithMessage("Exactly one option must be marked as correct for MultipleChoice/TrueFalse.");
    }
}
