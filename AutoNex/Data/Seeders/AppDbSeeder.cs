using AutoNex.Enums;
using AutoNex.Helpers;
using Microsoft.Extensions.Options;

namespace AutoNex.Data.Seeders;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IOptions<SeedSettings> seedSettings)
    {
        var settings = seedSettings.Value;

        if (settings.Users.Count > 0 && !await db.Users.AnyAsync())
        {
            foreach (var seedUser in settings.Users)
            {
                var role = Enum.Parse<UserRole>(seedUser.Role);
                db.Users.Add(new User
                {
                    FullName = seedUser.FullName,
                    Email = seedUser.Email.ToLowerInvariant().Trim(),
                    PasswordHash = PasswordHelper.Hash(seedUser.Password),
                    Role = role,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync();
        }

        if (settings.Services.Count > 0 && !await db.Services.AnyAsync())
        {
            db.Services.AddRange(settings.Services.Select(s => new Service
            {
                Name = s.Name,
                Description = s.Description,
                DefaultPrice = s.DefaultPrice,
                MinKmInterval = s.MinKmInterval,
                MaxKmInterval = s.MaxKmInterval,
                MinMonth = s.MinMonth,
                MaxMonth = s.MaxMonth
            }));

            await db.SaveChangesAsync();
        }

        if (settings.ToolCategories.Count > 0)
        {
            var existingNames = await db.ToolCategories.Select(tc => tc.Name).ToListAsync();
            var newCategories = settings.ToolCategories
                .Where(tc => !existingNames.Contains(tc.Name))
                .Select(tc => new ToolCategory { Name = tc.Name })
                .ToList();

            if (newCategories.Count > 0)
            {
                db.ToolCategories.AddRange(newCategories);
                await db.SaveChangesAsync();
            }
        }

        if (settings.Suppliers.Count > 0 && !await db.Suppliers.AnyAsync())
        {
            db.Suppliers.AddRange(settings.Suppliers.Select(s => new Supplier
            {
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email
            }));

            await db.SaveChangesAsync();
        }

        if (settings.Currencies.Count > 0)
        {
            var existingCodes = await db.Currencies.Select(c => c.IsoCode).ToListAsync();
            var newCurrencies = settings.Currencies
                .Where(c => !existingCodes.Contains(c.IsoCode))
                .Select(c => new Currency
                {
                    IsoCode = c.IsoCode,
                    Name = c.Name,
                    Symbol = c.Symbol,
                    IsPrincipal = c.IsPrincipal,
                    IsActive = true
                })
                .ToList();

            if (newCurrencies.Count > 0)
            {
                db.Currencies.AddRange(newCurrencies);
                await db.SaveChangesAsync();
            }
        }

        if (!await db.MessageTemplates.AnyAsync())
        {
            db.MessageTemplates.Add(new MessageTemplate
            {
                Key = "mileage_alert_reminder",
                Template = "🚗 {WorkshopName} | Tu asistente de confianza\nHola {ClientName}!! esperamos que estés teniendo un buen día 😊\n\nQueremos recordarte que tu vehículo ({Brand} {Model} - {LicensePlate}) está próximo a su mantenimiento de {ServiceName}.\n\nAtenderlo a tiempo te ayudará a mantenerlo en óptimas condiciones y evitar contratiempos.\n\n{WorkshopAddress}\n{WorkshopPhone}",
                Description = "Template para recordatorios de alertas de kilometraje"
            });

            await db.SaveChangesAsync();
        }

        if (settings.Settings.Count > 0)
        {
            var existingKeys = await db.Settings.Select(s => s.Key).ToListAsync();
            var newSettings = settings.Settings
                .Where(s => !existingKeys.Contains(s.Key))
                .Select(s => new Setting
                {
                    Key = s.Key,
                    Value = s.Value,
                    Type = s.Type,
                    Description = s.Description,
                    IsActive = true
                })
                .ToList();

            if (newSettings.Count > 0)
            {
                db.Settings.AddRange(newSettings);
                await db.SaveChangesAsync();
            }
        }
    }
}
