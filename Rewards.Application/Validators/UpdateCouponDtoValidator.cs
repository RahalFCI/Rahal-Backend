using FluentValidation;
using Rewards.Application.DTOs.Coupons;
using Rewards.Domain.Enums;

namespace Rewards.Application.Validators
{
    public class UpdateCouponDtoValidator : AbstractValidator<UpdateCouponDto>
    {
        public UpdateCouponDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.XpCost).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DiscountValue).GreaterThan(0);
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .When(x => x.DiscountType == CouponDiscountType.Percentage);
            RuleFor(x => x.MaxDiscountValue).GreaterThan(0).When(x => x.MaxDiscountValue.HasValue);
            RuleFor(x => x.MinimumCharge).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxClaims).GreaterThan(0);
        }
    }
}
