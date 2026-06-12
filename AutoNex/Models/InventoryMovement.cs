using AutoNex.Enums;

namespace AutoNex.Models;

public class InventoryMovement
{
    public int Id { get; set; }
    public int? ConsumableId { get; set; }
    public int? ToolId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Consumable? Consumable { get; set; }
    public Tool? Tool { get; set; }
}
