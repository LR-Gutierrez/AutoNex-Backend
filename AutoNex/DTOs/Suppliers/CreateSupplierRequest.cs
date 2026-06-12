namespace AutoNex.DTOs.Suppliers;

public record CreateSupplierRequest(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email
);
