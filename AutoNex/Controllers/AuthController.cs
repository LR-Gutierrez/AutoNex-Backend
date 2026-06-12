using AutoNex.DTOs.Auth;
using AutoNex.Helpers;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("AuthWindow")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Created(string.Empty, ApiResponse<AuthResponse>.Ok(result, "Usuario registrado exitosamente"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Inicio de sesión exitoso"));
    }
}
