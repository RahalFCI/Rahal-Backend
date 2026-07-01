using Shared.Domain.Entities;

namespace Rewards.Domain.Entities
{
    public class TravelPlan : BaseEntity
    {
        public Guid ExplorerId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }
        public decimal BudgetLimit { get; set; }
        public int StayDurationDays { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string GeneratedPlan { get; set; } = "{}";
    }
}
