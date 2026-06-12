namespace AutoNex.DTOs.Tools;

public record ToolResponse(
    int Id,
    string Name,
    string Category,
    int Quantity,
    string Status,
    DateTime? PurchaseDate,
    DateTime CreatedAt
);
