namespace AutoNex.DTOs.Auth;

public record AuthResponse(
    int UserId,
    string FullName,
    string Email,
    string Role,
    string Token
);
