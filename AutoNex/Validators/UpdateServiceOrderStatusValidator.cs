using AutoNex.DTOs.ServiceOrders;
using AutoNex.Enums;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateServiceOrderStatusValidator : AbstractValidator<UpdateServiceOrderStatusRequest>
{
    public UpdateServiceOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("El estado de orden de servicio no es válido")
            .NotEqual(ServiceOrderStatus.Paid).WithMessage("No puede establecer el estado a Pagado directamente, use el endpoint de pago");
    }
}
