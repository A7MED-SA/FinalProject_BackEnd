using Athary.Application.DTOs.Certificate;
using FluentValidation;

namespace Athary.Application.Validators.Certificate;

public class IssueCertificateValidator : AbstractValidator<IssueCertificateRequest>
{
    public IssueCertificateValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.EnrollmentId).NotEmpty();
    }
}
