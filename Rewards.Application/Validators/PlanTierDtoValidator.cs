using FluentValidation;
using Rewards.Application.DTOs.PlanTiers;

namespace Rewards.Application.Validators
{
    public class CreatePlanTierDtoValidator : AbstractValidator<CreatePlanTierDto>
    {
        public CreatePlanTierDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.WeeklyPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.WeeklyXpCost).GreaterThanOrEqualTo(0);
            RuleFor(x => x.XpMultiplier).GreaterThan(0);
            RuleFor(x => x.MaxTravelPlans).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdatePlanTierDtoValidator : AbstractValidator<UpdatePlanTierDto>
    {
        public UpdatePlanTierDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.WeeklyPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.WeeklyXpCost).GreaterThanOrEqualTo(0);
            RuleFor(x => x.XpMultiplier).GreaterThan(0);
            RuleFor(x => x.MaxTravelPlans).GreaterThanOrEqualTo(0);
        }
    }
}
