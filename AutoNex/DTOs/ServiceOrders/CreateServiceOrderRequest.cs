namespace AutoNex.DTOs.ServiceOrders;

public record CreateServiceOrderRequest
{
    public int VehicleId { get; set; }
    public int ClientId { get; set; }
    public int CurrentKm { get; set; }
    public int? EstimatedDailyKm { get; set; }
    public int? DaysPerWeek { get; set; }
    public string? Notes { get; set; }
    public List<CreateServiceOrderItemRequest> Items { get; set; } = [];
}

public class CreateServiceOrderItemRequest
{
    public string Type { get; set; } = "Service";
    public int? ServiceId { get; set; }
    public int? ConsumableId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
