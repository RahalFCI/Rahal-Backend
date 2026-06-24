namespace Rewards.Application.DTOs.Coupons
{
    public class CouponSearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public Guid? VendorId { get; set; }
        public string? DiscountType { get; set; }
        public int? MaxXpCost { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
