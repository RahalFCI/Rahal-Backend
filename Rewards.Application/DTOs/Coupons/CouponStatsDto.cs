namespace Rewards.Application.DTOs.Coupons
{
    public class CouponStatsDto
    {
        public Guid CouponId { get; set; }
        public int TotalClaims { get; set; }
        public int RedeemedCount { get; set; }
        public int ClaimedCount { get; set; }
        public int PendingCount { get; set; }
        public int ExpiredCount { get; set; }
        public int CancelledCount { get; set; }
        public double RedemptionRate { get; set; }
        public DateTime? LastRedeemedAt { get; set; }
    }
}
