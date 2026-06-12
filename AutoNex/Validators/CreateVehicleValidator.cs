using AutoNex.DTOs.Vehicles;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0).WithMessage("El cliente es obligatorio");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es obligatoria")
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("El año no es válido");

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("La placa es obligatoria")
            .MaximumLength(20);

        RuleFor(x => x.VIN)
            .MaximumLength(17).When(x => !string.IsNullOrEmpty(x.VIN))
            .WithMessage("El VIN debe tener máximo 17 caracteres");
    }
}
