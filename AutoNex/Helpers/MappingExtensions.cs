using AutoNex.DTOs.Clients;
using AutoNex.DTOs.Suppliers;
using AutoNex.DTOs.Vehicles;
using AutoNex.Models;

namespace AutoNex.Helpers;

public static class MappingExtensions
{
    public static ClientResponse ToResponse(this Client client)
        => new(
            client.Id,
            client.FullName,
            client.Phone,
            client.Email,
            client.Address,
            client.CreatedAt,
            client.Vehicles?.Select(v => v.ToBriefResponse()).ToList()
        );

    public static VehicleBriefResponse ToBriefResponse(this Vehicle vehicle)
        => new(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.LicensePlate
        );

    public static VehicleResponse ToResponse(this Vehicle vehicle)
        => new(
            vehicle.Id,
            vehicle.ClientId,
            vehicle.Client.FullName,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.LicensePlate,
            vehicle.VIN,
            vehicle.CreatedAt
        );

    public static SupplierResponse ToResponse(this Supplier supplier)
        => new(
            supplier.Id,
            supplier.Name,
            supplier.ContactPerson,
            supplier.Phone,
            supplier.Email,
            supplier.CreatedAt
        );
}
