using AutoNex.Enums;

namespace AutoNex.DTOs.Tools;

public record UpdateToolRequest
{
    public string Name { get; set; } = string.Empty;
    public int ToolCategoryId { get; set; }
    public int Quantity { get; set; }
    public ToolStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
}
