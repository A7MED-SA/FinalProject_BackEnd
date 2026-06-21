using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Domain.Enums;
using FluentValidation;

namespace Athary.Application.Validators.InstructorRequests;

public class ProcessInstructorRequestValidator : AbstractValidator<ProcessInstructorRequestDto>
{
    public ProcessInstructorRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("الحالة غير صالحة");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(500)
            .WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف");

        RuleFor(x => x.RejectionReason)
            .MaximumLength(500)
            .WithMessage("سبب الرفض يجب ألا يتجاوز 500 حرف");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => x.Status == InstructorRequestStatus.Rejected)
            .WithMessage("سبب الرفض مطلوب عند رفض الطلب");
    }
}
