using FluentValidation;
using Gamification.Application.DTOs.ExplorerAchievement;

namespace Gamification.Application.Validators.ExplorerAchievement
{
    public class CreateExplorerAchievementDtoValidator : AbstractValidator<CreateExplorerAchievementDto>
    {
        public CreateExplorerAchievementDtoValidator()
        {
            RuleFor(x => x.AchievementId)
                .NotEmpty().WithMessage("Achievement ID is required");

            RuleFor(x => x.ExplorerId)
                .NotEmpty().WithMessage("Explorer ID is required");
        }
    }
}
