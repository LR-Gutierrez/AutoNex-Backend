namespace AutoNex.DTOs.Vehicles;

public record UpdateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string? VIN
);
