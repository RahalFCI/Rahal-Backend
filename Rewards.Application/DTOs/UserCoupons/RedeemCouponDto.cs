namespace Rewards.Application.DTOs.UserCoupons
{
    public class RedeemCouponDto
    {
        public Guid VendorId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
