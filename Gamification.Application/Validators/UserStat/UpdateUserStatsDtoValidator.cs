using FluentValidation;
using Gamification.Application.DTOs.UserStats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Validators.UserStat
{
    public class UpdateUserStatsDtoValidator : AbstractValidator<UpdateUserStatsDto>
    {
        public UpdateUserStatsDtoValidator()
        {
            RuleFor(x => x.TotalCheckIns)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total check-ins must be greater than or equal to 0.");

            RuleFor(x => x.TotalChallengesCompleted)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total challenges completed must be greater than or equal to 0.");

            RuleFor(x => x.TotalAchievementsEarned)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total achievements earned must be greater than or equal to 0.");

            RuleFor(x => x.TotalBadgesEarned)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total badges earned must be greater than or equal to 0.");

            RuleFor(x => x.LongestStreak)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Longest streak must be greater than or equal to 0.");
        }
    }
}
