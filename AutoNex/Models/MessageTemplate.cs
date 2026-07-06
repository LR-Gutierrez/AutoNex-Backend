namespace AutoNex.Models;

public class MessageTemplate
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
