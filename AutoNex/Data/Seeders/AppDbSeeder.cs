using AutoNex.Models;

namespace AutoNex.Data.Seeders;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Currencies.AnyAsync())
        {
            db.Currencies.AddRange(
                new Currency { IsoCode = "USD", Name = "Dólar estadounidense", Symbol = "$", IsPrincipal = true, IsActive = true },
                new Currency { IsoCode = "EUR", Name = "Euro", Symbol = "€", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "CNY", Name = "Yuan chino", Symbol = "¥", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "RUB", Name = "Rublo ruso", Symbol = "₽", IsPrincipal = false, IsActive = true },
                new Currency { IsoCode = "TRY", Name = "Lira turca", Symbol = "₺", IsPrincipal = false, IsActive = true }
            );
        }

        if (!await db.Settings.AnyAsync())
        {
            db.Settings.AddRange(
                new Setting { Key = "bcv_auto_consult", Value = "false", Type = "boolean", Description = "Activa la consulta automática al BCV" },
                new Setting { Key = "bcv_update_cron", Value = "10 16 * * 1-5", Type = "string", Description = "Lun-Vie 4:10 PM VET" },
                new Setting { Key = "bcv_audit_enabled", Value = "true", Type = "boolean", Description = "Activa la auditoría diaria de tasas" },
                new Setting { Key = "bcv_audit_cron", Value = "0 18 * * *", Type = "string", Description = "6:00 PM VET" }
            );
        }

        await db.SaveChangesAsync();
    }
}
