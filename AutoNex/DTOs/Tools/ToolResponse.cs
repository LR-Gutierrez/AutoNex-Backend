namespace AutoNex.DTOs.Tools;

public record ToolResponse(
    int Id,
    string Name,
    int ToolCategoryId,
    string CategoryName,
    int Quantity,
    string Status,
    DateTime? PurchaseDate,
    DateTime CreatedAt
);
