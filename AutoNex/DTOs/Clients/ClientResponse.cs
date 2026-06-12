namespace AutoNex.DTOs.Clients;

public record ClientResponse(
    int Id,
    string FullName,
    string Phone,
    string? Email,
    string? Address,
    DateTime CreatedAt,
    List<VehicleBriefResponse>? Vehicles
);

public record VehicleBriefResponse(
    int Id,
    string Brand,
    string Model,
    int Year,
    string LicensePlate
);
