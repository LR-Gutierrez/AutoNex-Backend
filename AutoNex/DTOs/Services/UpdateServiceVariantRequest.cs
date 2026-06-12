namespace AutoNex.DTOs.Services;

public record UpdateServiceVariantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinKmInterval { get; set; }
    public int MaxKmInterval { get; set; }
    public int? RecommendedMonths { get; set; }
}
