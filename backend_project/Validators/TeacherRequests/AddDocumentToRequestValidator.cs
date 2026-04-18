using FluentValidation;
using backend_project.Models;
using backend_project.DTOs.TeacherRequests.Requests;

namespace backend_project.Validators.TeacherRequests;

public class AddDocumentToRequestValidator : AbstractValidator<AddDocumentToRequestDto>
{
    public AddDocumentToRequestValidator()
    {
        RuleFor(x => x.DocumentType)
            .IsInEnum()
            .WithMessage("نوع المستند غير صالح");

        RuleFor(x => x.FileId)
            .NotEmpty()
            .When(x => x.DocumentType == DocumentType.CV || x.DocumentType == DocumentType.Certificate)
            .WithMessage("معرف الملف مطلوب لهذا النوع من المستندات");

        RuleFor(x => x.UrlValue)
            .NotEmpty()
            .When(x => x.DocumentType == DocumentType.PortfolioLink)
            .WithMessage("الرابط مطلوب لنوع المحفظة الإلكترونية")
            .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("رابط غير صالح");
    }
}