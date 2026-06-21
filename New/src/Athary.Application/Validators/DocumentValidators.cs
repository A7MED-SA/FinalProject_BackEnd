using Athary.Application.DTOs.Courses;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FileId).NotEmpty();
    }
}

public sealed class UpdateDocumentValidator : AbstractValidator<UpdateDocumentDto>
{
    public UpdateDocumentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
    }
}
