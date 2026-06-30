using Rewards.Domain.Enums;

namespace Rewards.Application.DTOs.Coupons
{
    public class UpdateCouponDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpCost { get; set; }
        public CouponDiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal MinimumCharge { get; set; }
        public int MaxClaims { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
