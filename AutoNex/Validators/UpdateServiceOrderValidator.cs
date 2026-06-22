using AutoNex.DTOs.ServiceOrders;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateServiceOrderValidator : AbstractValidator<UpdateServiceOrderRequest>
{
    public UpdateServiceOrderValidator()
    {
        RuleFor(x => x.CurrentKm)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo");

        When(x => x.EstimatedDailyKm.HasValue || x.DaysPerWeek.HasValue, () =>
        {
            RuleFor(x => x.EstimatedDailyKm!.Value)
                .GreaterThan(0).WithMessage("El kilometraje diario estimado debe ser mayor a 0")
                .When(x => x.EstimatedDailyKm.HasValue);

            RuleFor(x => x.DaysPerWeek!.Value)
                .InclusiveBetween(1, 7).WithMessage("Los días por semana deben estar entre 1 y 7")
                .When(x => x.DaysPerWeek.HasValue);

            RuleFor(x => x)
                .Must(x => x.EstimatedDailyKm.HasValue && x.DaysPerWeek.HasValue)
                .WithMessage("Debe especificar tanto el kilometraje diario como los días por semana");
        });

        When(x => x.ApplyLaborPercentage, () =>
        {
            RuleFor(x => x.LaborPercentage)
                .NotNull().WithMessage("Debe especificar el porcentaje de mano de obra")
                .InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100");
        });

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe incluir al menos un servicio");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ServiceId)
                .GreaterThan(0).WithMessage("El servicio es obligatorio");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");
        });
    }
}
