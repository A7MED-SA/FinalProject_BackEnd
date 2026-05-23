using FluentValidation;
using backend_project.DTOs.Cart;

namespace backend_project.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");
    }
}
