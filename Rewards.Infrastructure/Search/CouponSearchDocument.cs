using System.Text.Json.Serialization;

namespace Rewards.Infrastructure.Search
{
    public class CouponSearchDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("vendorId")]
        public string VendorId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("xpCost")]
        public int XpCost { get; set; }

        [JsonPropertyName("discountType")]
        public string DiscountType { get; set; } = string.Empty;

        [JsonPropertyName("discountValue")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("minimumCharge")]
        public decimal MinimumCharge { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("remainingClaims")]
        public int RemainingClaims { get; set; }
    }
}
