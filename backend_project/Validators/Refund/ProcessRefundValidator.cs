using FluentValidation;
using backend_project.DTOs.Refund;

namespace backend_project.Validators.Refund;

public class ProcessRefundValidator : AbstractValidator<ProcessRefundRequest>
{
    public ProcessRefundValidator()
    {
        RuleFor(x => x.RefundId)
            .NotEmpty().WithMessage("RefundId is required.");
    }
}
