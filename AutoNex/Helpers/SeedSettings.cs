namespace AutoNex.Helpers;

public class SeedSettings
{
    public const string SectionName = "SeedData";
    public List<SeedUser> Users { get; init; } = [];
    public List<SeedService> Services { get; init; } = [];
    public List<SeedToolCategory> ToolCategories { get; init; } = [];
    public List<SeedSupplier> Suppliers { get; init; } = [];
    public List<SeedCurrency> Currencies { get; init; } = [];
    public List<SeedMessageTemplate> MessageTemplates { get; init; } = [];
    public SeedWorkshopInfo? WorkshopInfo { get; init; }
    public List<SeedSettingItem> Settings { get; init; } = [];
}

public class SeedUser
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public class SeedService
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal DefaultPrice { get; init; }
    public int? MinKmInterval { get; init; }
    public int? MaxKmInterval { get; init; }
    public int? MinMonth { get; init; }
    public int? MaxMonth { get; init; }
}

public class SeedToolCategory
{
    public string Name { get; init; } = string.Empty;
}

public class SeedSupplier
{
    public string Name { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class SeedCurrency
{
    public string IsoCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public bool IsPrincipal { get; init; }
}

public class SeedMessageTemplate
{
    public string Key { get; init; } = string.Empty;
    public string Template { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public class SeedWorkshopInfo
{
    public string BusinessName { get; init; } = string.Empty;
    public string? Rif { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? MapsUrl { get; init; }
    public string? Phone { get; init; }
    public string? SecondaryPhone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? OpeningHours { get; init; }
}

public class SeedSettingItem
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public string? Description { get; init; }
}
