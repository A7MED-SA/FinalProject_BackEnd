using FluentValidation;
using backend_project.Models;
using backend_project.DTOs.TeacherRequests.Requests;

namespace backend_project.Validators.TeacherRequests;

public class SubmitTeacherRequestValidator : AbstractValidator<SubmitTeacherRequestDto>
{
    public SubmitTeacherRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("الرسالة مطلوبة")
            .MaximumLength(1000).WithMessage("الرسالة يجب ألا تتجاوز 1000 حرف");

        RuleFor(x => x.Documents)
            .NotEmpty().WithMessage("يجب إرفاق مستند واحد على الأقل")
            .Must(documents => documents.Count <= 10)
            .WithMessage("لا يمكن إرفاق أكثر من 10 مستندات");

        RuleForEach(x => x.Documents)
            .SetValidator(new TeacherRequestDocumentValidator());
    }
}

public class TeacherRequestDocumentValidator : AbstractValidator<TeacherRequestDocumentDto>
{
    public TeacherRequestDocumentValidator()
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