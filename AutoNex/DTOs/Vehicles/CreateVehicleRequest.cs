namespace AutoNex.DTOs.Vehicles;

public record CreateVehicleRequest(
    int ClientId,
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN
);
