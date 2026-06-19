using AutoNex.DTOs.ServiceOrders;
using AutoNex.Enums;
using FluentValidation;

namespace AutoNex.Validators;

public class PayOrderValidator : AbstractValidator<PayOrderRequest>
{
    public PayOrderValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("El método de pago no es válido");

        When(x => x.PaymentMethod is PaymentMethod.PagoMovil or PaymentMethod.Transferencia, () =>
        {
            RuleFor(x => x.OperationNumber)
                .NotEmpty().WithMessage("El número de operación es obligatorio para pagos electrónicos");

            RuleFor(x => x.OperationDate)
                .NotNull().WithMessage("La fecha de operación es obligatoria para pagos electrónicos");
        });
    }
}
