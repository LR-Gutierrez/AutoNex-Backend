using AutoNex.Enums;

namespace AutoNex.Models;

public class Tool
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ToolCategory Category { get; set; }
    public int Quantity { get; set; }
    public ToolStatus Status { get; set; } = ToolStatus.Available;
    public DateTime? PurchaseDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
