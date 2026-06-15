namespace AutoNex.DTOs.ServiceOrders;

public record UpdateServiceOrderRequest
{
    public int CurrentKm { get; set; }
    public int? EstimatedDailyKm { get; set; }
    public int? DaysPerWeek { get; set; }
    public string? Notes { get; set; }
    public List<CreateServiceOrderItemRequest> Items { get; set; } = [];
}
