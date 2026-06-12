using AutoNex.DTOs;
using AutoNex.DTOs.ServiceOrders;

namespace AutoNex.Services.Interfaces;

public interface IServiceOrderService
{
    Task<PagedResponse<ServiceOrderResponse>> GetAllAsync(DateTime? from, DateTime? to, int? clientId, int? vehicleId, string? status, int? page, int? pageSize);
    Task<ServiceOrderResponse?> GetByIdAsync(int id);
    Task<ServiceOrderResponse> CreateAsync(CreateServiceOrderRequest request, int userId);
    Task<ServiceOrderResponse?> UpdateAsync(int id, UpdateServiceOrderRequest request);
    Task<ServiceOrderResponse?> UpdateStatusAsync(int id, UpdateServiceOrderStatusRequest request);
}
