using FluentValidation;
using backend_project.DTOs.Coupon;

namespace backend_project.Validators.Coupon;

public class ApplyCouponValidator : AbstractValidator<ValidateCouponRequest>
{
    public ApplyCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}
