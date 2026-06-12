using AutoNex.Enums;

namespace AutoNex.DTOs.Auth;

public record RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Phone { get; set; }
}
