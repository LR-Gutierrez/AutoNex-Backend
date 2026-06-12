using AutoNex.DTOs.Services;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateServiceVariantValidator : AbstractValidator<CreateServiceVariantRequest>
{
    public CreateServiceVariantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.MinKmInterval)
            .GreaterThanOrEqualTo(0).WithMessage("El intervalo mínimo no puede ser negativo");

        RuleFor(x => x.MaxKmInterval)
            .GreaterThan(0).WithMessage("El intervalo máximo debe ser mayor a 0")
            .GreaterThanOrEqualTo(x => x.MinKmInterval)
            .WithMessage("El intervalo máximo debe ser mayor o igual al mínimo");

        RuleFor(x => x.RecommendedMonths)
            .GreaterThan(0).When(x => x.RecommendedMonths.HasValue)
            .WithMessage("Los meses recomendados deben ser mayor a 0");
    }
}
