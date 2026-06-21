using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Domain.Enums;
using FluentValidation;

namespace Athary.Application.Validators.InstructorRequests;

public class SubmitInstructorRequestValidator : AbstractValidator<SubmitInstructorRequestDto>
{
    public SubmitInstructorRequestValidator()
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

public class InstructorRequestDocumentValidator : AbstractValidator<InstructorRequestDocumentDto>
{
    public InstructorRequestDocumentValidator()
    {
        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("نوع المستند غير صالح");

        RuleFor(x => x.FileId)
            .NotEmpty()
            .When(x => x.DocumentType == DocumentType.CV || x.DocumentType == DocumentType.Certificate)
            .WithMessage("معرف الملف مطلوب لهذا النوع من المستندات");

        RuleFor(x => x.UrlValue)
            .NotEmpty()
            .When(x => x.DocumentType == DocumentType.PortfolioLink)
            .WithMessage("الرابط مطلوب لنوع المحفظة الإلكترونية")
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .When(x => x.DocumentType == DocumentType.PortfolioLink)
            .WithMessage("رابط غير صالح");
    }
}
