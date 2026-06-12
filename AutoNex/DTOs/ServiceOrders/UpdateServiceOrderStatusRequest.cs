using AutoNex.Enums;

namespace AutoNex.DTOs.ServiceOrders;

public class UpdateServiceOrderStatusRequest
{
    public ServiceOrderStatus Status { get; set; }
}
