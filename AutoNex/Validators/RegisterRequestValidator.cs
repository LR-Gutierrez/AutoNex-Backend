using AutoNex.DTOs.Auth;
using FluentValidation;

namespace AutoNex.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("El email no es válido")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una minúscula")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("El rol no es válido");
    }
}
