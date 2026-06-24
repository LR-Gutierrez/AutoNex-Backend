namespace AutoNex.DTOs.MessageTemplates;

public record UpdateMessageTemplateRequest
{
    public string Template { get; set; } = string.Empty;
    public string? Description { get; set; }
}
