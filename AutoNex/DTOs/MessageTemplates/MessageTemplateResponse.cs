namespace AutoNex.DTOs.MessageTemplates;

public record MessageTemplateResponse
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
