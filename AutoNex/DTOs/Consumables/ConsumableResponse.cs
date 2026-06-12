namespace AutoNex.DTOs.Consumables;

public record ConsumableResponse(
    int Id,
    string Name,
    string Category,
    int StockQuantity,
    int MinStock,
    decimal UnitPrice,
    int? SupplierId,
    string? SupplierName,
    DateTime CreatedAt
);
