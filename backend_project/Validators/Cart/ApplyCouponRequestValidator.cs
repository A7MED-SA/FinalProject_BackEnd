using FluentValidation;
using backend_project.DTOs.Cart;

namespace backend_project.Validators.Cart;

public class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest>
{
    public ApplyCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}
