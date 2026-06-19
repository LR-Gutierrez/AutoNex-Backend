namespace AutoNex.Helpers;

public class SeedSettings
{
    public const string SectionName = "SeedData";
    public List<SeedUser> Users { get; init; } = [];
}

public class SeedUser
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
