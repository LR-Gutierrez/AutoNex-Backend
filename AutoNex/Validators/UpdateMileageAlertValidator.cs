using AutoNex.DTOs.MileageAlerts;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateMileageAlertValidator : AbstractValidator<UpdateMileageAlertRequest>
{
    public UpdateMileageAlertValidator()
    {
        RuleFor(x => x.EstimatedWeeklyKm)
            .GreaterThan(0).WithMessage("El kilometraje semanal estimado debe ser mayor a cero");
    }
}
