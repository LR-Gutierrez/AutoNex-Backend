using AutoNex.DTOs.Clients;
using AutoNex.DTOs.Consumables;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.DTOs.InventoryMovements;
using AutoNex.DTOs.MileageAlerts;
using AutoNex.DTOs.Notifications;
using AutoNex.DTOs.Services;
using AutoNex.DTOs.Suppliers;
using AutoNex.DTOs.ToolCategories;
using AutoNex.DTOs.Tools;
using AutoNex.DTOs.Users;
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

    public static VehicleResponse ToResponse(this Vehicle vehicle, List<ServiceOrderBriefResponse>? orders = null)
        => new(
            vehicle.Id,
            vehicle.ClientId,
            vehicle.Client.FullName,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.LicensePlate,
            vehicle.VIN,
            vehicle.CreatedAt,
            orders ?? []
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
            tool.ToolCategoryId,
            tool.ToolCategory.Name,
            tool.Quantity,
            tool.Status.ToString(),
            tool.PurchaseDate,
            tool.CreatedAt
        );

    public static ToolCategoryResponse ToResponse(this ToolCategory category)
        => new(
            category.Id,
            category.Name,
            category.CreatedAt
        );

    public static ServiceResponse ToResponse(this Service service)
        => new(
            service.Id,
            service.Name,
            service.Description,
            service.DefaultPrice,
            service.MinKmInterval,
            service.MaxKmInterval,
            service.MinMonth,
            service.MaxMonth,
            service.CreatedAt
        );

    public static InventoryMovementResponse ToResponse(this InventoryMovement movement)
        => new(
            movement.Id,
            movement.ConsumableId,
            movement.Consumable?.Name,
            movement.ToolId,
            movement.Tool?.Name,
            movement.MovementType,
            movement.Quantity,
            movement.Reference,
            movement.ReferenceId,
            movement.Notes,
            movement.CreatedAt
        );

    public static UserResponse ToResponse(this User user)
        => new(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Phone,
            user.IsActive,
            user.CreatedAt
        );

    public static NotificationResponse ToResponse(this Notification notification)
    {
        var vehicleInfo = notification.Vehicle is not null
            ? $"{notification.Vehicle.Brand} {notification.Vehicle.Model} ({notification.Vehicle.LicensePlate})"
            : null;

        return new NotificationResponse(
            notification.Id,
            notification.ClientId,
            notification.Client.FullName,
            notification.VehicleId,
            vehicleInfo,
            notification.Type,
            notification.Recipient,
            notification.Message,
            notification.SentAt,
            notification.Status,
            notification.CreatedAt
        );
    }

    public static FinancialRecordResponse ToResponse(this FinancialRecord record)
        => new(
            record.Id,
            record.Type,
            record.Category,
            record.Amount,
            record.AmountInBs,
            record.ExchangeRateValue,
            record.Description,
            record.Date,
            record.UserId,
            record.User.FullName,
            record.CreatedAt
        );

    public static MileageAlertResponse ToResponse(this MileageAlert alert, int currentKm)
    {
        var remainingKm = alert.NextAlertKm - currentKm;
        var isDue = alert.IsActive && currentKm + (alert.EstimatedWeeklyKm * 2) >= alert.NextAlertKm;

        return new MileageAlertResponse(
            alert.Id,
            alert.VehicleId,
            $"{alert.Vehicle.Brand} {alert.Vehicle.Model} ({alert.Vehicle.LicensePlate})",
            alert.ServiceId,
            alert.Service?.Name ?? "",
            currentKm,
            alert.EstimatedWeeklyKm,
            alert.NextAlertKm,
            remainingKm > 0 ? remainingKm : 0,
            isDue,
            alert.LastAlertDate,
            alert.NextAlertDate,
            alert.IsActive,
            alert.CreatedAt
        );
    }
}
