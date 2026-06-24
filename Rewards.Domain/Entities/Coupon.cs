using Rewards.Domain.Enums;
using Shared.Domain.Entities;

namespace Rewards.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public Guid VendorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpCost { get; set; }
        public CouponDiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal MinimumCharge { get; set; }
        public int MaxClaims { get; set; }
        public int CurrentClaims { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    }
}
