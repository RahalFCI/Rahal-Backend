namespace Rewards.Application.DTOs.Coupons
{
    public class GetCouponDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpCost { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal MinimumCharge { get; set; }
        public int MaxClaims { get; set; }
        public int CurrentClaims { get; set; }
        public int RemainingClaims { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
