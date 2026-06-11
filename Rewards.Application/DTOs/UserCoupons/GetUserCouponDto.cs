namespace Rewards.Application.DTOs.UserCoupons
{
    public class GetUserCouponDto
    {
        public Guid Id { get; set; }
        public Guid ExplorerId { get; set; }
        public Guid CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsRedeemed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ClaimedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string CouponTitle { get; set; } = string.Empty;
    }
}
