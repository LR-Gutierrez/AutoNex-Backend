using AutoNex.DTOs.Consumables;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateConsumableValidator : AbstractValidator<CreateConsumableRequest>
{
    public CreateConsumableValidator()
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
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");
    }
}
