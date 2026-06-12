using AutoNex.Enums;

namespace AutoNex.DTOs.Tools;

public record CreateToolRequest
{
    public string Name { get; set; } = string.Empty;
    public ToolCategory Category { get; set; }
    public int Quantity { get; set; }
    public ToolStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
}
