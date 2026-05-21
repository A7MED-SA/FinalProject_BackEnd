using FluentValidation;
using backend_project.DTOs.VideoComment;

namespace backend_project.Validators;

public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}
