using AutoNex.Enums;

namespace AutoNex.DTOs.Consumables;

public record CreateConsumableRequest
{
    public string Name { get; set; } = string.Empty;
    public ConsumableCategory Category { get; set; }
    public int StockQuantity { get; set; }
    public int MinStock { get; set; }
    public decimal UnitPrice { get; set; }
    public int? SupplierId { get; set; }
}
