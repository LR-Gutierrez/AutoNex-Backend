using AutoNex.DTOs.Services;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateServiceValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DefaultPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");

        RuleFor(x => x.MinKmInterval)
            .GreaterThan(0).When(x => x.MinKmInterval.HasValue)
            .WithMessage("El intervalo mínimo de km debe ser mayor a cero");

        RuleFor(x => x.MaxKmInterval)
            .GreaterThan(0).When(x => x.MaxKmInterval.HasValue)
            .WithMessage("El intervalo máximo de km debe ser mayor a cero");

        RuleFor(x => x.MinMonth)
            .GreaterThan(0).When(x => x.MinMonth.HasValue)
            .WithMessage("El mes mínimo debe ser mayor a cero");

        RuleFor(x => x.MaxMonth)
            .GreaterThan(0).When(x => x.MaxMonth.HasValue)
            .WithMessage("El mes máximo debe ser mayor a cero");
    }
}
