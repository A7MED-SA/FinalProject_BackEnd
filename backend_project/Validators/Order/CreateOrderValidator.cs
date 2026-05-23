using FluentValidation;
using backend_project.DTOs.Order;

namespace backend_project.Validators.Order;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        When(x => x.CouponId.HasValue, () =>
        {
            RuleFor(x => x.CouponId!.Value)
                .NotEmpty().WithMessage("CouponId must not be empty.");
        });
    }
}
