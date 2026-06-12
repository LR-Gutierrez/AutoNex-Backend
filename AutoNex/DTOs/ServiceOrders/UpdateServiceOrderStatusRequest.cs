using AutoNex.Enums;

namespace AutoNex.DTOs.ServiceOrders;

public record UpdateServiceOrderStatusRequest
{
    public ServiceOrderStatus Status { get; set; }
}
