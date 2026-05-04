using FluentValidation;
using Gamification.Application.DTOs.UserStats;

namespace Gamification.Application.Validators
{
    public class CreateUserStatsDtoValidator : AbstractValidator<CreateUserStatsDto>
    {
        public CreateUserStatsDtoValidator()
        {
            RuleFor(x => x.ExplorerId)
                .NotEmpty().WithMessage("Explorer ID is required");
        }
    }
}
