using AutoNex.Enums;

namespace AutoNex.DTOs.Users;

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    UserRole Role,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt
);
