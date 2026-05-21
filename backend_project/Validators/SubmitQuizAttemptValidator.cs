using FluentValidation;
using backend_project.DTOs.QuizAttempt;

namespace backend_project.Validators;

public class SubmitQuizAttemptValidator : AbstractValidator<SubmitAttemptDto>
{
    public SubmitQuizAttemptValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Answers are required.");

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .NotEmpty().WithMessage("QuestionId is required for each answer.");
            });
    }
}
