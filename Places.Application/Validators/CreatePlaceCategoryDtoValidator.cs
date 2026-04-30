using FluentValidation;
using Places.Application.DTOs.PlaceCategory;

namespace Places.Application.Validators
{
    public class CreatePlaceCategoryDtoValidator : AbstractValidator<CreatePlaceCategoryDto>
    {
        public CreatePlaceCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(150).WithMessage("Category name cannot exceed 150 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Category description is required");
        }
    }
}
