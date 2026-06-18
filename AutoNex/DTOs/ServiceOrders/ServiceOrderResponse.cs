using AutoNex.Enums;

namespace AutoNex.DTOs.ServiceOrders;

public record ServiceOrderResponse(
    int Id,
    int VehicleId,
    string VehicleInfo,
    int ClientId,
    string ClientName,
    int UserId,
    string UserName,
    int CurrentKm,
    int? EstimatedDailyKm,
    int? DaysPerWeek,
    DateTime Date,
    ServiceOrderStatus Status,
    decimal TotalAmount,
    string? Notes,
    PaymentMethod? PaymentMethod,
    string? OperationNumber,
    DateTime? OperationDate,
    DateTime CreatedAt,
    List<ServiceOrderItemResponse> Items
);

public record ServiceOrderItemResponse(
    int Id,
    string Type,
    int? ServiceId,
    string? ServiceName,
    int? ConsumableId,
    string? ConsumableName,
    int Quantity,
    decimal UnitPrice
);
