namespace AutoNex.DTOs.Services;

public record UpdateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public int? MinKmInterval { get; set; }
    public int? MaxKmInterval { get; set; }
    public int? MinMonth { get; set; }
    public int? MaxMonth { get; set; }
}
