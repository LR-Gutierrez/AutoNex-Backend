using AutoNex.DTOs.Notifications;
using FluentValidation;

namespace AutoNex.Validators;

public class SendNotificationValidator : AbstractValidator<SendNotificationRequest>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("El cliente es obligatorio");

        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("El destinatario es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("El mensaje es obligatorio")
            .MaximumLength(1000);
    }
}
