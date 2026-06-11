using FluentValidation;
using Rewards.Application.DTOs.UserCoupons;

namespace Rewards.Application.Validators
{
    public class RedeemCouponDtoValidator : AbstractValidator<RedeemCouponDto>
    {
        public RedeemCouponDtoValidator()
        {
            RuleFor(x => x.VendorId).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        }
    }
}
