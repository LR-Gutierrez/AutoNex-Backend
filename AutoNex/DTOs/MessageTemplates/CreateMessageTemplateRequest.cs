namespace AutoNex.DTOs.MessageTemplates;

public record CreateMessageTemplateRequest
{
    public string Key { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string? Description { get; set; }
}
