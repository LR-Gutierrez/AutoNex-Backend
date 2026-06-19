using AutoNex.Data;
using AutoNex.DTOs.ExchangeRates;
using AutoNex.Enums;
using AutoNex.Hubs;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("bcv")]
    public async Task<IActionResult> GetBcvSettings()
    {
        var settings = await _db.Settings
            .AsNoTracking()
            .Where(s => s.Key.StartsWith("bcv_") && s.IsActive)
            .ToListAsync();

        return Ok(settings.ToDictionary(s => s.Key, s => s.Value));
    }

    [HttpPut("bcv")]
    public async Task<IActionResult> UpdateBcvSettings(Dictionary<string, string> updates)
    {
        foreach (var (key, value) in updates)
        {
            var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Configuración actualizada" });
    }
}
