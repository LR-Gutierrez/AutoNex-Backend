namespace AutoNex.DTOs.Clients;

public record CreateClientRequest(
    string FullName,
    string Phone,
    string? Email,
    string? Address
);
