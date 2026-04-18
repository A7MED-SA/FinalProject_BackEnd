using FluentValidation;
using backend_project.Models;
using backend_project.DTOs.TeacherRequests.Requests;

namespace backend_project.Validators.TeacherRequests;

public class ProcessTeacherRequestValidator : AbstractValidator<ProcessTeacherRequestDto>
{
    public ProcessTeacherRequestValidator()
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

        // التحقق من أن سبب الرفض مطلوب عند الرفض
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .When(x => x.Status == TeacherRequestStatus.Rejected)
            .WithMessage("سبب الرفض مطلوب عند رفض الطلب");
    }
}