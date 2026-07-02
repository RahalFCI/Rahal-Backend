using Rewards.Domain.Enums;

namespace Rewards.Application.DTOs.Subscriptions
{
    public class PurchaseSubscriptionDto
    {
        public Guid PlanTierId { get; set; }
        public SubscriptionPaymentMethod PaymentMethod { get; set; }
        public int Duration { get; set; }
    }
}
