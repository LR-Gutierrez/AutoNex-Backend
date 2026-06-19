using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoNex.Data;
using AutoNex.DTOs.Auth;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoNex.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
            throw new InvalidOperationException("El email ya está registrado");

        var user = new User
        {
            FullName = request.FullName,
            Email = normalizedEmail,
            PasswordHash = PasswordHelper.Hash(request.Password),
            Role = request.Role,
            Phone = request.Phone
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            GenerateToken(user)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Usuario desactivado");

        if (!PasswordHelper.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            GenerateToken(user)
        );
    }

    private string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("permission", "read"),
            new("permission", "write"),
            new("permission", "authorize"),
            new("permission", "disable"),
            new("permission", "enable"),
            new("permission", "autoupdate-bcv")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
