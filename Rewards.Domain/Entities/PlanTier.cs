using Shared.Domain.Entities;

namespace Rewards.Domain.Entities
{
    public class PlanTier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal WeeklyPrice { get; set; }
        public int WeeklyXpCost { get; set; }
        public decimal XpMultiplier { get; set; } = 1m;
        public int MaxTravelPlans { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
