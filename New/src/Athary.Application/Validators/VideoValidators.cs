using Athary.Application.DTOs.Courses;
using Athary.Domain.Enums;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateVideoValidator : AbstractValidator<CreateVideoDto>
{
    public CreateVideoValidator()
    {
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.VideoFileId).NotEmpty();
        RuleFor(x => x.DurationSeconds).GreaterThan(0);
        RuleFor(x => x.Provider).IsInEnum();
    }
}

public sealed class UpdateVideoValidator : AbstractValidator<UpdateVideoDto>
{
    public UpdateVideoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
    }
}
