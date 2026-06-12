using AutoNex.DTOs.Clients;
using AutoNex.DTOs.Consumables;
using AutoNex.DTOs.Services;
using AutoNex.DTOs.Suppliers;
using AutoNex.DTOs.Tools;
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

    public static ConsumableResponse ToResponse(this Consumable consumable)
        => new(
            consumable.Id,
            consumable.Name,
            consumable.Category.ToString(),
            consumable.StockQuantity,
            consumable.MinStock,
            consumable.UnitPrice,
            consumable.SupplierId,
            consumable.Supplier?.Name,
            consumable.CreatedAt
        );

    public static ToolResponse ToResponse(this Tool tool)
        => new(
            tool.Id,
            tool.Name,
            tool.Category.ToString(),
            tool.Quantity,
            tool.Status.ToString(),
            tool.PurchaseDate,
            tool.CreatedAt
        );

    public static ServiceResponse ToResponse(this Service service)
        => new(
            service.Id,
            service.Name,
            service.Description,
            service.DefaultPrice,
            service.CreatedAt
        );
}
