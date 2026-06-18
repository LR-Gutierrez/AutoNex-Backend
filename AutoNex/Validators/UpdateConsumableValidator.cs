using AutoNex.DTOs.Consumables;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateConsumableValidator : AbstractValidator<UpdateConsumableRequest>
{
    public UpdateConsumableValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("La categoría no es válida");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo")
            .PrecisionScale(18, 2, true);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).When(x => x.SupplierId.HasValue)
            .WithMessage("El proveedor no es válido");
    }
}
