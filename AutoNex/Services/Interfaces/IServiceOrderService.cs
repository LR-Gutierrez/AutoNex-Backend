using AutoNex.DTOs.ServiceOrders;

namespace AutoNex.Services.Interfaces;

public interface IServiceOrderService
{
    Task<List<ServiceOrderResponse>> GetAllAsync(DateTime? from, DateTime? to, int? clientId, int? vehicleId, string? status);
    Task<ServiceOrderResponse?> GetByIdAsync(int id);
    Task<ServiceOrderResponse> CreateAsync(CreateServiceOrderRequest request, int userId);
    Task<ServiceOrderResponse?> UpdateAsync(int id, UpdateServiceOrderRequest request);
    Task<ServiceOrderResponse?> UpdateStatusAsync(int id, UpdateServiceOrderStatusRequest request);
}
