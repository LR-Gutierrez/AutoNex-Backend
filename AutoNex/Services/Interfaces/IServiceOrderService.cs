using AutoNex.DTOs;
using AutoNex.DTOs.ServiceOrders;

namespace AutoNex.Services.Interfaces;

public interface IServiceOrderService
{
    Task<PagedResponse<ServiceOrderResponse>> GetAllAsync(DateTime? from, DateTime? to, int? clientId, int? vehicleId, string? status, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<ServiceOrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceOrderResponse> CreateAsync(CreateServiceOrderRequest request, int userId, CancellationToken cancellationToken = default);
    Task<ServiceOrderResponse?> UpdateAsync(int id, UpdateServiceOrderRequest request, CancellationToken cancellationToken = default);
    Task<ServiceOrderResponse?> UpdateStatusAsync(int id, UpdateServiceOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<ServiceOrderResponse> PayAsync(int id, int userId, PayOrderRequest request, CancellationToken cancellationToken = default);
}
