using AutoNex.Enums;

namespace AutoNex.DTOs.Users;

public record UpdateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    string? Phone
);
