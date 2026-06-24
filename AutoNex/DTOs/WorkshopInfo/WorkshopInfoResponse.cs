namespace AutoNex.DTOs.WorkshopInfo;

public record WorkshopInfoResponse
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? Rif { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? MapsUrl { get; set; }
    public string? Phone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? OpeningHours { get; set; }
    public DateTime UpdatedAt { get; set; }
}
