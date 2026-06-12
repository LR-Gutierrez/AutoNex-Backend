namespace AutoNex.DTOs.ServiceOrders;

public class CreateServiceOrderRequest
{
    public int VehicleId { get; set; }
    public int ClientId { get; set; }
    public int CurrentKm { get; set; }
    public string? Notes { get; set; }
    public List<CreateServiceOrderItemRequest> Items { get; set; } = [];
}

public class CreateServiceOrderItemRequest
{
    public int ServiceId { get; set; }
    public int? ServiceVariantId { get; set; }
    public int? ConsumableId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
