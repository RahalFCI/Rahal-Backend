using FluentValidation;
using Rewards.Application.DTOs.Subscriptions;

namespace Rewards.Application.Validators
{
    public class PurchaseSubscriptionDtoValidator : AbstractValidator<PurchaseSubscriptionDto>
    {
        public PurchaseSubscriptionDtoValidator()
        {
            RuleFor(x => x.PlanTierId).NotEmpty();
            RuleFor(x => x.PaymentMethod).IsInEnum();
        }
    }
}
