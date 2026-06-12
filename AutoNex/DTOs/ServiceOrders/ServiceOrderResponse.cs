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
    DateTime Date,
    ServiceOrderStatus Status,
    decimal TotalAmount,
    string? Notes,
    DateTime CreatedAt,
    List<ServiceOrderItemResponse> Items
);

public record ServiceOrderItemResponse(
    int Id,
    int ServiceId,
    string ServiceName,
    int? ServiceVariantId,
    string? ServiceVariantName,
    int? ConsumableId,
    string? ConsumableName,
    int Quantity,
    decimal UnitPrice
);
