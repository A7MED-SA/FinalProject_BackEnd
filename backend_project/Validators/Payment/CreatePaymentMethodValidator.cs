using FluentValidation;
using backend_project.DTOs.Payment;

namespace backend_project.Validators.Payment;

public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodRequest>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}
