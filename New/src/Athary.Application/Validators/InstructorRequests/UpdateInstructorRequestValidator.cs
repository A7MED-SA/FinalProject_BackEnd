using Athary.Application.DTOs.InstructorRequests.Requests;
using FluentValidation;

namespace Athary.Application.Validators.InstructorRequests;

public class UpdateInstructorRequestValidator : AbstractValidator<UpdateInstructorRequestDto>
{
    public UpdateInstructorRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("الرسالة مطلوبة")
            .MaximumLength(1000).WithMessage("الرسالة يجب ألا تتجاوز 1000 حرف");

        RuleFor(x => x.Documents)
            .NotEmpty().WithMessage("يجب إرفاق مستند واحد على الأقل")
            .Must(documents => documents.Count <= 10)
            .WithMessage("لا يمكن إرفاق أكثر من 10 مستندات");

        RuleForEach(x => x.Documents)
            .SetValidator(new InstructorRequestDocumentValidator());
    }
}
