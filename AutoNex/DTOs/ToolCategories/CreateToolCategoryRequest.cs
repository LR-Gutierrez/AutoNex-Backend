namespace AutoNex.DTOs.ToolCategories;

public record CreateToolCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}
