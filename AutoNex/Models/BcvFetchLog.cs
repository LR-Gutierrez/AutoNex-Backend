namespace AutoNex.Models;

public class BcvFetchLog
{
    public int Id { get; set; }
    public DateTime ValueDate { get; set; }
    public string? RatesJson { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public string Action { get; set; } = "Unknown";
    public string FetchedBy { get; set; } = "Auto";
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
