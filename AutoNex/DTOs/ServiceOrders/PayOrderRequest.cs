using AutoNex.Enums;

namespace AutoNex.DTOs.ServiceOrders;

public record PayOrderRequest(
    PaymentMethod PaymentMethod,
    string? OperationNumber,
    DateTime? OperationDate,
    decimal? AmountInBs = null
);
