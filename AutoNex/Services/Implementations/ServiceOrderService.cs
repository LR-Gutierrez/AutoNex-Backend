using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.ServiceOrders;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class ServiceOrderService : IServiceOrderService
{
    private readonly AppDbContext _context;

    public ServiceOrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ServiceOrderResponse>> GetAllAsync(DateTime? from, DateTime? to, int? clientId, int? vehicleId, string? status, int? page, int? pageSize)
    {
        var query = _context.ServiceOrders
            .Include(o => o.Client)
            .Include(o => o.Vehicle)
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Service)
            .Include(o => o.Items)
                .ThenInclude(i => i.Consumable)
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.Date >= from.Value);
        if (to.HasValue) query = query.Where(o => o.Date <= to.Value);
        if (clientId.HasValue) query = query.Where(o => o.ClientId == clientId.Value);
        if (vehicleId.HasValue) query = query.Where(o => o.VehicleId == vehicleId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ServiceOrderStatus>(status, true, out var s))
            query = query.Where(o => o.Status == s);

        query = query
            .Where(o => !o.Vehicle.IsDeleted)
            .Where(o => !o.Client.IsDeleted)
            .OrderByDescending(o => o.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, MapToResponse);
    }

    public async Task<ServiceOrderResponse?> GetByIdAsync(int id)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Client)
            .Include(o => o.Vehicle)
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Service)
            .Include(o => o.Items)
                .ThenInclude(i => i.Consumable)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : MapToResponse(order);
    }

    public async Task<ServiceOrderResponse> CreateAsync(CreateServiceOrderRequest request, int userId)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId)
            ?? throw new KeyNotFoundException("Vehículo no encontrado");
        var client = await _context.Clients.FindAsync(request.ClientId)
            ?? throw new KeyNotFoundException("Cliente no encontrado");

        var order = new ServiceOrder
        {
            VehicleId = request.VehicleId,
            ClientId = request.ClientId,
            UserId = userId,
            CurrentKm = request.CurrentKm,
            EstimatedDailyKm = request.EstimatedDailyKm,
            DaysPerWeek = request.DaysPerWeek,
            Notes = request.Notes,
            Status = ServiceOrderStatus.Open
        };

        decimal total = 0;
        foreach (var itemReq in request.Items)
        {
            if (itemReq.Type == "Service")
            {
                if (!itemReq.ServiceId.HasValue)
                    throw new InvalidOperationException("ServiceId es obligatorio para items de tipo Service");

                var service = await _context.Services.FindAsync(itemReq.ServiceId.Value)
                    ?? throw new KeyNotFoundException($"Servicio {itemReq.ServiceId} no encontrado");
            }

            if (itemReq.ConsumableId.HasValue)
            {
                var consumable = await _context.Consumables.FindAsync(itemReq.ConsumableId.Value)
                    ?? throw new KeyNotFoundException($"Consumible {itemReq.ConsumableId} no encontrado");

                if (consumable.StockQuantity < itemReq.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para {consumable.Name}");

                consumable.StockQuantity -= itemReq.Quantity;
                consumable.UpdatedAt = DateTime.UtcNow;
            }

            var item = new ServiceOrderItem
            {
                ServiceId = itemReq.Type == "Service" ? itemReq.ServiceId : null,
                ConsumableId = itemReq.ConsumableId,
                Quantity = itemReq.Quantity,
                UnitPrice = itemReq.UnitPrice
            };

            order.Items.Add(item);
            total += itemReq.UnitPrice * itemReq.Quantity;
        }

        order.TotalAmount = total;

        _context.ServiceOrders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(order.Id))!;
    }

    public async Task<ServiceOrderResponse?> UpdateAsync(int id, UpdateServiceOrderRequest request)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;

        if (order.Status == ServiceOrderStatus.Completed || order.Status == ServiceOrderStatus.Cancelled)
            throw new InvalidOperationException("No se puede modificar una orden completada o cancelada");

        // Restore stock from previous items
        foreach (var oldItem in order.Items)
        {
            if (oldItem.ConsumableId.HasValue)
            {
                var consumable = await _context.Consumables.FindAsync(oldItem.ConsumableId.Value);
                if (consumable is not null)
                {
                    consumable.StockQuantity += oldItem.Quantity;
                    consumable.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        _context.ServiceOrderItems.RemoveRange(order.Items);
        order.Items.Clear();

        order.CurrentKm = request.CurrentKm;
        order.EstimatedDailyKm = request.EstimatedDailyKm;
        order.DaysPerWeek = request.DaysPerWeek;
        order.Notes = request.Notes;

        decimal total = 0;
        foreach (var itemReq in request.Items)
        {
            if (itemReq.Type == "Service")
            {
                if (!itemReq.ServiceId.HasValue)
                    throw new InvalidOperationException("ServiceId es obligatorio para items de tipo Service");

                var service = await _context.Services.FindAsync(itemReq.ServiceId.Value)
                    ?? throw new KeyNotFoundException($"Servicio {itemReq.ServiceId} no encontrado");
            }

            if (itemReq.ConsumableId.HasValue)
            {
                var consumable = await _context.Consumables.FindAsync(itemReq.ConsumableId.Value)
                    ?? throw new KeyNotFoundException($"Consumible {itemReq.ConsumableId} no encontrado");

                if (consumable.StockQuantity < itemReq.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para {consumable.Name}");

                consumable.StockQuantity -= itemReq.Quantity;
                consumable.UpdatedAt = DateTime.UtcNow;
            }

            var item = new ServiceOrderItem
            {
                ServiceId = itemReq.Type == "Service" ? itemReq.ServiceId : null,
                ConsumableId = itemReq.ConsumableId,
                Quantity = itemReq.Quantity,
                UnitPrice = itemReq.UnitPrice
            };

            order.Items.Add(item);
            total += itemReq.UnitPrice * itemReq.Quantity;
        }

        order.TotalAmount = total;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<ServiceOrderResponse?> UpdateStatusAsync(int id, UpdateServiceOrderStatusRequest request)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return null;

        if (order.Status == ServiceOrderStatus.Completed || order.Status == ServiceOrderStatus.Cancelled)
            throw new InvalidOperationException("No se puede cambiar el estado de una orden completada o cancelada");

        if (request.Status == ServiceOrderStatus.Cancelled)
        {
            foreach (var item in order.Items.Where(i => i.ConsumableId.HasValue))
            {
                var consumableId = item.ConsumableId.GetValueOrDefault();
                var consumable = await _context.Consumables.FindAsync(consumableId);
                if (consumable is not null)
                {
                    consumable.StockQuantity += item.Quantity;
                    consumable.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    private static ServiceOrderResponse MapToResponse(ServiceOrder order)
    {
        return new ServiceOrderResponse(
            order.Id,
            order.VehicleId,
            $"{order.Vehicle.Brand} {order.Vehicle.Model} ({order.Vehicle.LicensePlate})",
            order.ClientId,
            order.Client.FullName,
            order.UserId,
            order.User.FullName,
            order.CurrentKm,
            order.EstimatedDailyKm,
            order.DaysPerWeek,
            order.Date,
            order.Status,
            order.TotalAmount,
            order.Notes,
            order.CreatedAt,
            order.Items.Select(i => new ServiceOrderItemResponse(
                i.Id,
                i.ServiceId.HasValue ? "Service" : "Consumable",
                i.ServiceId,
                i.Service?.Name,
                i.ConsumableId,
                i.Consumable?.Name,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );
    }
}
