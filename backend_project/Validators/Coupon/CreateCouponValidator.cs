using FluentValidation;
using backend_project.DTOs.Coupon;

namespace backend_project.Validators.Coupon;

public class CreateCouponValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Type)
            .Must(t => t == "Percentage" || t == "Fixed")
            .WithMessage("Type must be 'Percentage' or 'Fixed'.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0.");

        When(x => x.Type == "Percentage", () =>
        {
            RuleFor(x => x.Value)
                .LessThanOrEqualTo(100).WithMessage("Percentage value must not exceed 100.");
        });

        RuleFor(x => x.ApplicableTo)
            .Must(a => a == "All" || a == "SpecificCourses" || a == "Category")
            .WithMessage("ApplicableTo must be 'All', 'SpecificCourses', or 'Category'.");
    }
}
