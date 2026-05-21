using FluentValidation;
using backend_project.DTOs.Video;

namespace backend_project.Validators;

public class CreateVideoValidator : AbstractValidator<CreateVideoDto>
{
    public CreateVideoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.VideoFileId)
            .NotEmpty().WithMessage("VideoFileId is required.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("DurationSeconds must be greater than 0.");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Invalid video provider.");
    }
}
