namespace AutoNex.Models;

public class ServiceOrderItem
{
    public int Id { get; set; }
    public int ServiceOrderId { get; set; }
    public int ServiceId { get; set; }
    public int? ConsumableId { get; set; }
    public int? ServiceVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public Service? Service { get; set; }
    public Consumable? Consumable { get; set; }
    public ServiceVariant? ServiceVariant { get; set; }
}
