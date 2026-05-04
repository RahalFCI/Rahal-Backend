using FluentValidation;
using Gamification.Application.DTOs.Achievement;

namespace Gamification.Application.Validators
{
    public class UpdateAchievementDtoValidator : AbstractValidator<UpdateAchievementDto>
    {
        public UpdateAchievementDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Achievement title is required")
                .MaximumLength(200).WithMessage("Achievement title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Achievement description is required")
                .MaximumLength(500).WithMessage("Achievement description cannot exceed 500 characters");

            RuleFor(x => x.BadgeId)
                .NotEmpty().WithMessage("Badge ID is required");

            RuleFor(x => x.XpReward)
                .GreaterThan(0).WithMessage("XP reward must be greater than 0");

            RuleFor(x => x.CriteriaTypeId)
                .NotEmpty().WithMessage("Criteria type ID is required");

            RuleFor(x => x.CriteriaThreshold)
                .GreaterThan(0).WithMessage("Criteria threshold must be greater than 0");
        }
    }
}
