using FluentValidation;
using backend_project.DTOs.Payment;

namespace backend_project.Validators.Payment;

public class ProcessPaymentValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("PaymentMethodId is required.");
    }
}
