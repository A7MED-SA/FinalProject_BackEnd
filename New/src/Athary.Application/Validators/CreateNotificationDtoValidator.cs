using Athary.Application.DTOs.Notification;
using FluentValidation;

namespace Athary.Application.Validators;

public sealed class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Message).MaximumLength(1000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.LinkUrl).MaximumLength(500);
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
