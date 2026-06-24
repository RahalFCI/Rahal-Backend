using Rewards.Domain.Enums;
using Shared.Domain.Entities;

namespace Rewards.Domain.Entities
{
    public class UserCoupon : BaseEntity
    {
        public Guid ExplorerId { get; set; }
        public Guid CouponId { get; set; }
        public Coupon? Coupon { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsRedeemed { get; set; }
        public UserCouponStatus Status { get; set; } = UserCouponStatus.Pending;
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RedeemedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
