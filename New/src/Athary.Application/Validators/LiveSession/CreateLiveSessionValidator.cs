using Athary.Application.DTOs.LiveSession;
using FluentValidation;

namespace Athary.Application.Validators.LiveSession;

public class CreateLiveSessionValidator : AbstractValidator<CreateLiveSessionDto>
{
    public CreateLiveSessionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.MeetingUrl)
            .NotEmpty().WithMessage("MeetingUrl is required.")
            .MaximumLength(2000).WithMessage("MeetingUrl must not exceed 2000 characters.");

        RuleFor(x => x.ScheduledStart)
            .LessThan(x => x.ScheduledEnd).WithMessage("ScheduledStart must be before ScheduledEnd.");

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0).When(x => x.MaxAttendees.HasValue)
            .WithMessage("MaxAttendees must be greater than 0.");
    }
}
