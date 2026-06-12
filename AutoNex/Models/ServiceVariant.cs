namespace AutoNex.Models;

public class ServiceVariant
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinKmInterval { get; set; }
    public int MaxKmInterval { get; set; }
    public int? RecommendedMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Service Service { get; set; } = null!;
}
