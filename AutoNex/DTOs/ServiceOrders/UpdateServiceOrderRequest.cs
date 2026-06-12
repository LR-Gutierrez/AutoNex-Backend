namespace AutoNex.DTOs.ServiceOrders;

public class UpdateServiceOrderRequest
{
    public int CurrentKm { get; set; }
    public string? Notes { get; set; }
    public List<CreateServiceOrderItemRequest> Items { get; set; } = [];
}
