using Rewards.Domain.Enums;
using Shared.Domain.Entities;

namespace Rewards.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid ExplorerId { get; set; }
        public Guid PlanTierId { get; set; }
        public PlanTier? PlanTier { get; set; }
        public SubscriptionPaymentMethod PaymentMethod { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public DateTime? StartedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
