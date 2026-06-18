using AutoNex.DTOs.Suppliers;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(200);

        RuleFor(x => x.ContactPerson)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.Phone)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("El email no es válido")
            .MaximumLength(200);
    }
}
