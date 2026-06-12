using AutoNex.Enums;

namespace AutoNex.Models;

public class Consumable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ConsumableCategory Category { get; set; }
    public int StockQuantity { get; set; }
    public int MinStock { get; set; }
    public decimal UnitPrice { get; set; }
    public int? SupplierId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Supplier? Supplier { get; set; }
}
