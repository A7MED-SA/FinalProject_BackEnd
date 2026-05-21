using FluentValidation;
using backend_project.DTOs.Enrollment;
using backend_project.Models;

namespace backend_project.Validators;

public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Invalid enrollment source.");
    }
}
