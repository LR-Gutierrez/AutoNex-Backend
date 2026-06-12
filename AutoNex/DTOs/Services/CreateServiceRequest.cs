namespace AutoNex.DTOs.Services;

public record CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public int? RecommendedKmInterval { get; set; }
}
