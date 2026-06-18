using AutoNex.Data;
using AutoNex.DTOs.Users;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        return users.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        return user?.ToResponse();
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (user is null) return null;

        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("El email ya está en uso por otro usuario");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (user is null) return false;

        user.IsActive = false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
