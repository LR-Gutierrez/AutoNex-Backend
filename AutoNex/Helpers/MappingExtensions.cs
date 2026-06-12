using AutoNex.DTOs.Clients;
using AutoNex.DTOs.Consumables;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.DTOs.MileageAlerts;
using AutoNex.DTOs.Notifications;
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
            service.RecommendedKmInterval,
            service.CreatedAt
        );

    public static ServiceVariantResponse ToResponse(this ServiceVariant variant)
        => new(
            variant.Id,
            variant.ServiceId,
            variant.Service.Name,
            variant.Name,
            variant.Description,
            variant.MinKmInterval,
            variant.MaxKmInterval,
            variant.RecommendedMonths,
            variant.IsActive,
            variant.CreatedAt
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
            record.Description,
            record.Date,
            record.UserId,
            record.User.FullName,
            record.CreatedAt
        );

    public static MileageAlertResponse ToResponse(this MileageAlert alert, int? currentKm = null)
    {
        var effectiveKm = currentKm ?? alert.LastRecordedKm;
        var remainingKm = alert.NextAlertKm - effectiveKm;
        var isDue = alert.IsActive && effectiveKm + (alert.EstimatedWeeklyKm * 2) >= alert.NextAlertKm;

        return new MileageAlertResponse(
            alert.Id,
            alert.VehicleId,
            $"{alert.Vehicle.Brand} {alert.Vehicle.Model} ({alert.Vehicle.LicensePlate})",
            alert.LastRecordedKm,
            alert.EstimatedWeeklyKm,
            alert.NextAlertKm,
            remainingKm > 0 ? remainingKm : 0,
            isDue,
            alert.LastAlertDate,
            alert.IsActive,
            alert.CreatedAt
        );
    }
}
