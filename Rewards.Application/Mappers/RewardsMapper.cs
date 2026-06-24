using Rewards.Application.DTOs.Coupons;
using Rewards.Application.DTOs.PlanTiers;
using Rewards.Application.DTOs.Subscriptions;
using Rewards.Application.DTOs.TravelPlans;
using Rewards.Application.DTOs.UserCoupons;
using Rewards.Domain.Entities;

namespace Rewards.Application.Mappers
{
    public static class RewardsMapper
    {
        public static GetCouponDto ToDto(Coupon coupon)
        {
            return new GetCouponDto
            {
                Id = coupon.Id,
                VendorId = coupon.VendorId,
                Title = coupon.Title,
                Description = coupon.Description,
                XpCost = coupon.XpCost,
                DiscountType = coupon.DiscountType.ToString(),
                DiscountValue = coupon.DiscountValue,
                MaxDiscountValue = coupon.MaxDiscountValue,
                MinimumCharge = coupon.MinimumCharge,
                MaxClaims = coupon.MaxClaims,
                CurrentClaims = coupon.CurrentClaims,
                RemainingClaims = Math.Max(0, coupon.MaxClaims - coupon.CurrentClaims),
                ExpiresAt = coupon.ExpiresAt,
                IsActive = coupon.IsActive
            };
        }

        public static Coupon ToEntity(CreateCouponDto dto)
        {
            return new Coupon
            {
                VendorId = dto.VendorId,
                Title = dto.Title,
                Description = dto.Description,
                XpCost = dto.XpCost,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MaxDiscountValue = dto.MaxDiscountValue,
                MinimumCharge = dto.MinimumCharge,
                MaxClaims = dto.MaxClaims,
                ExpiresAt = dto.ExpiresAt,
                IsActive = dto.IsActive
            };
        }

        public static void Update(Coupon coupon, UpdateCouponDto dto)
        {
            coupon.Title = dto.Title;
            coupon.Description = dto.Description;
            coupon.XpCost = dto.XpCost;
            coupon.DiscountType = dto.DiscountType;
            coupon.DiscountValue = dto.DiscountValue;
            coupon.MaxDiscountValue = dto.MaxDiscountValue;
            coupon.MinimumCharge = dto.MinimumCharge;
            coupon.MaxClaims = dto.MaxClaims;
            coupon.ExpiresAt = dto.ExpiresAt;
            coupon.IsActive = dto.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;
        }

        public static GetUserCouponDto ToDto(UserCoupon userCoupon)
        {
            return new GetUserCouponDto
            {
                Id = userCoupon.Id,
                ExplorerId = userCoupon.ExplorerId,
                CouponId = userCoupon.CouponId,
                Code = userCoupon.Code,
                IsRedeemed = userCoupon.IsRedeemed,
                Status = userCoupon.Status.ToString(),
                ClaimedAt = userCoupon.ClaimedAt,
                RedeemedAt = userCoupon.RedeemedAt,
                ExpiresAt = userCoupon.ExpiresAt,
                CouponTitle = userCoupon.Coupon?.Title ?? string.Empty
            };
        }

        public static GetPlanTierDto ToDto(PlanTier planTier)
        {
            return new GetPlanTierDto
            {
                Id = planTier.Id,
                Name = planTier.Name,
                Description = planTier.Description,
                WeeklyPrice = planTier.WeeklyPrice,
                WeeklyXpCost = planTier.WeeklyXpCost,
                XpMultiplier = planTier.XpMultiplier,
                MaxTravelPlans = planTier.MaxTravelPlans,
                IsActive = planTier.IsActive
            };
        }

        public static PlanTier ToEntity(CreatePlanTierDto dto)
        {
            return new PlanTier
            {
                Name = dto.Name,
                Description = dto.Description,
                WeeklyPrice = dto.WeeklyPrice,
                WeeklyXpCost = dto.WeeklyXpCost,
                XpMultiplier = dto.XpMultiplier,
                MaxTravelPlans = dto.MaxTravelPlans,
                IsActive = dto.IsActive
            };
        }

        public static void Update(PlanTier planTier, UpdatePlanTierDto dto)
        {
            planTier.Name = dto.Name;
            planTier.Description = dto.Description;
            planTier.WeeklyPrice = dto.WeeklyPrice;
            planTier.WeeklyXpCost = dto.WeeklyXpCost;
            planTier.XpMultiplier = dto.XpMultiplier;
            planTier.MaxTravelPlans = dto.MaxTravelPlans;
            planTier.IsActive = dto.IsActive;
            planTier.UpdatedAt = DateTime.UtcNow;
        }

        public static GetSubscriptionDto ToDto(Subscription subscription)
        {
            return new GetSubscriptionDto
            {
                Id = subscription.Id,
                ExplorerId = subscription.ExplorerId,
                PlanTierId = subscription.PlanTierId,
                PlanTierName = subscription.PlanTier?.Name ?? string.Empty,
                PaymentMethod = subscription.PaymentMethod.ToString(),
                Status = subscription.Status.ToString(),
                StartedAt = subscription.StartedAt,
                ExpiresAt = subscription.ExpiresAt,
                CancelledAt = subscription.CancelledAt
            };
        }

        public static GetTravelPlanDto ToDto(TravelPlan travelPlan)
        {
            return new GetTravelPlanDto
            {
                Id = travelPlan.Id,
                ExplorerId = travelPlan.ExplorerId,
                SubscriptionId = travelPlan.SubscriptionId,
                BudgetLimit = travelPlan.BudgetLimit,
                StayDurationDays = travelPlan.StayDurationDays,
                Prompt = travelPlan.Prompt,
                GeneratedPlanJson = travelPlan.GeneratedPlanJson,
                CreatedAt = travelPlan.CreatedAt
            };
        }
    }
}
