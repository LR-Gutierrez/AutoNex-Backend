using AutoNex.DTOs.Tools;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateToolValidator : AbstractValidator<CreateToolRequest>
{
    public CreateToolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("La categoría no es válida");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("El estado no es válido");
    }
}
