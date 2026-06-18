namespace AutoNex.DTOs.Vehicles;

public record VehicleResponse(
    int Id,
    int ClientId,
    string ClientName,
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN,
    DateTime CreatedAt,
    List<ServiceOrderBriefResponse> ServiceOrders
);
