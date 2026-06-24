namespace Rewards.Application.DTOs.Subscriptions
{
    public class GetSubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid ExplorerId { get; set; }
        public Guid PlanTierId { get; set; }
        public string PlanTierName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
