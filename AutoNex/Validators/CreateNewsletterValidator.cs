using AutoNex.DTOs.ExchangeRates;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateNewsletterValidator : AbstractValidator<CreateNewsletterRequest>
{
    public CreateNewsletterValidator()
    {
        RuleFor(x => x.PublishedAt)
            .NotEmpty().WithMessage("Fecha de publicación es requerida");

        RuleFor(x => x.ValueDate)
            .NotEmpty().WithMessage("Fecha valor es requerida");

        RuleFor(x => x.Rates)
            .NotEmpty().WithMessage("Debe incluir al menos una tasa");

        RuleForEach(x => x.Rates).ChildRules(rate =>
        {
            rate.RuleFor(r => r.CurrencyId)
                .GreaterThan(0).WithMessage("Moneda inválida");

            rate.RuleFor(r => r.Value)
                .GreaterThan(0).WithMessage("La tasa debe ser mayor a 0");
        });

        RuleFor(x => x.Rates.Select(r => r.CurrencyId))
            .Must(x => x.Distinct().Count() == x.Count())
            .WithMessage("No puede haber monedas duplicadas en el mismo boletín");
    }
}
