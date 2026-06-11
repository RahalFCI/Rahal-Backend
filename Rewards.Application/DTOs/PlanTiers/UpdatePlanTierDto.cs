namespace Rewards.Application.DTOs.PlanTiers
{
    public class UpdatePlanTierDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal WeeklyPrice { get; set; }
        public int WeeklyXpCost { get; set; }
        public decimal XpMultiplier { get; set; } = 1m;
        public int MaxTravelPlans { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
