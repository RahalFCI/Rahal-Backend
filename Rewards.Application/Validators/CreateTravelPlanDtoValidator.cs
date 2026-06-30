using FluentValidation;
using Rewards.Application.DTOs.TravelPlans;
using System.Text.Json;

namespace Rewards.Application.Validators
{
    public class CreateTravelPlanDtoValidator : AbstractValidator<CreateTravelPlanDto>
    {
        public CreateTravelPlanDtoValidator()
        {
            RuleFor(x => x.BudgetLimit).GreaterThanOrEqualTo(0);
            RuleFor(x => x.StayDurationDays).GreaterThan(0);
            RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);

        }
    }
}
