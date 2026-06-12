using AutoNex.DTOs.MileageAlerts;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateMileageAlertValidator : AbstractValidator<CreateMileageAlertRequest>
{
    public CreateMileageAlertValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("El vehículo es obligatorio");

        RuleFor(x => x.EstimatedWeeklyKm)
            .GreaterThan(0).WithMessage("El kilometraje semanal estimado debe ser mayor a 0")
            .LessThanOrEqualTo(5000).WithMessage("El kilometraje semanal estimado no puede exceder 5000 km");
    }
}
