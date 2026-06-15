using AutoNex.DTOs.ToolCategories;
using FluentValidation;

namespace AutoNex.Validators;

public class CreateToolCategoryValidator : AbstractValidator<CreateToolCategoryRequest>
{
    public CreateToolCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100);
    }
}
