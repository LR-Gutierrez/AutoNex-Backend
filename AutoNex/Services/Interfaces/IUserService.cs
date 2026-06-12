using AutoNex.DTOs.Users;

namespace AutoNex.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
}
