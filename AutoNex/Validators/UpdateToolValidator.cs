using AutoNex.DTOs.Tools;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateToolValidator : AbstractValidator<UpdateToolRequest>
{
    public UpdateToolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.ToolCategoryId)
            .GreaterThan(0).WithMessage("La categoría es obligatoria");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("El estado no es válido");
    }
}
