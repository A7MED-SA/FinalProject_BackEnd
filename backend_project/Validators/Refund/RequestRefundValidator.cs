using FluentValidation;
using backend_project.DTOs.Refund;

namespace backend_project.Validators.Refund;

public class RequestRefundValidator : AbstractValidator<RequestRefundRequest>
{
    public RequestRefundValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required.");
    }
}
