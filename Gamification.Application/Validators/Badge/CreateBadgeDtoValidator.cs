using FluentValidation;
using Gamification.Application.DTOs.Badge;

namespace Gamification.Application.Validators.Badge
{
    public class CreateBadgeDtoValidator : AbstractValidator<CreateBadgeDto>
    {
        public CreateBadgeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Badge name is required")
                .MaximumLength(100).WithMessage("Badge name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Badge description is required")
                .MaximumLength(500).WithMessage("Badge description cannot exceed 500 characters");

        }
    }
}
