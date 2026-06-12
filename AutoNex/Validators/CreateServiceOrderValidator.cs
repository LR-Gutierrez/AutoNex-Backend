using AutoNex.DTOs.ServiceOrders;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateServiceOrderValidator : AbstractValidator<CreateServiceOrderRequest>
{
    public CreateServiceOrderValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("El vehículo es obligatorio");

        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("El cliente es obligatorio");

        RuleFor(x => x.CurrentKm)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo");

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
