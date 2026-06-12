namespace AutoNex.DTOs.Clients;

public record UpdateClientRequest(
    string FullName,
    string Phone,
    string? Email,
    string? Address
);
