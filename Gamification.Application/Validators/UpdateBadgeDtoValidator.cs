using FluentValidation;
using Gamification.Application.DTOs.Badge;

namespace Gamification.Application.Validators
{
    public class UpdateBadgeDtoValidator : AbstractValidator<UpdateBadgeDto>
    {
        public UpdateBadgeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Badge name is required")
                .MaximumLength(100).WithMessage("Badge name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Badge description is required")
                .MaximumLength(500).WithMessage("Badge description cannot exceed 500 characters");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Badge image URL is required")
                .MaximumLength(500).WithMessage("Badge image URL cannot exceed 500 characters");
        }
    }
}
