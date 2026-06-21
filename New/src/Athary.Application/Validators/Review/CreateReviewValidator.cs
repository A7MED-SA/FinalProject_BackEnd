using Athary.Application.DTOs.Review;
using FluentValidation;

namespace Athary.Application.Validators.Review;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .When(x => x.Comment != null);
    }
}
