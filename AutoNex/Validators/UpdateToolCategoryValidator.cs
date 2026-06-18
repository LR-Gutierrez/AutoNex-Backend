using AutoNex.DTOs.ToolCategories;
using FluentValidation;

namespace AutoNex.Validators;

public class UpdateToolCategoryValidator : AbstractValidator<UpdateToolCategoryRequest>
{
    public UpdateToolCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100);
    }
}
