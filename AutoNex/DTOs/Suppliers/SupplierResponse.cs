namespace AutoNex.DTOs.Suppliers;

public record SupplierResponse(
    int Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    DateTime CreatedAt
);
