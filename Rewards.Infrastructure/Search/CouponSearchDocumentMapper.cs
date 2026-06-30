using Rewards.Domain.Entities;

namespace Rewards.Infrastructure.Search
{
    public static class CouponSearchDocumentMapper
    {
        public static CouponSearchDocument ToSearchDocument(Coupon coupon)
        {
            return new CouponSearchDocument
            {
                Id = coupon.Id.ToString(),
                VendorId = coupon.VendorId.ToString(),
                Title = coupon.Title,
                Description = coupon.Description,
                XpCost = coupon.XpCost,
                DiscountType = coupon.DiscountType.ToString(),
                DiscountValue = coupon.DiscountValue,
                MinimumCharge = coupon.MinimumCharge,
                ExpiresAt = coupon.ExpiresAt,
                IsActive = coupon.IsActive,
                RemainingClaims = Math.Max(0, coupon.MaxClaims - coupon.CurrentClaims)
            };
        }
    }
}
