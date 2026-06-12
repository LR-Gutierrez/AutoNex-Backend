namespace AutoNex.DTOs.Suppliers;

public record UpdateSupplierRequest(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email
);
