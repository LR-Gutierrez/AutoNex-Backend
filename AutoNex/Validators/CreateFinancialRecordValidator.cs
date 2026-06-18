using AutoNex.DTOs.FinancialRecords;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateFinancialRecordValidator : AbstractValidator<CreateFinancialRecordRequest>
{
    public CreateFinancialRecordValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero")
            .PrecisionScale(18, 2, true);

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));
    }
}
