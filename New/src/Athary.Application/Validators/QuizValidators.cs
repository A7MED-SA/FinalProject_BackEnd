using Athary.Application.DTOs.Courses;
using Athary.Domain.Enums;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateQuizValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizValidator()
    {
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PassingScorePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.MaxAttempts).GreaterThan(0).When(x => x.MaxAttempts.HasValue);
    }
}

public sealed class UpdateQuizValidator : AbstractValidator<UpdateQuizDto>
{
    public UpdateQuizValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PassingScorePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.MaxAttempts).GreaterThan(0).When(x => x.MaxAttempts.HasValue);
    }
}

public sealed class CreateQuestionValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionValidator()
    {
        RuleFor(x => x.QuestionText).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Points).GreaterThan(0);

        RuleFor(x => x.Options)
            .Must((dto, options) => dto.Type switch
            {
                QuestionType.TrueFalse => options.Count == 2,
                QuestionType.MultipleChoice => options.Count >= 2,
                _ => true
            })
            .WithMessage("At least 2 options required for MultipleChoice; exactly 2 for TrueFalse.");

        RuleFor(x => x.Options)
            .Must(options => options.Count(o => o.IsCorrect) == 1)
            .When(x => x.Type != QuestionType.ShortAnswer)
            .WithMessage("Exactly one option must be correct for MultipleChoice/TrueFalse.");
    }
}

public sealed class SubmitAttemptValidator : AbstractValidator<SubmitAttemptDto>
{
    public SubmitAttemptValidator()
    {
        RuleFor(x => x.Answers).NotEmpty();
        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
        });
    }
}
