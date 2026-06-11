namespace Rewards.Application.DTOs.PlanTiers
{
    public class GetPlanTierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal WeeklyPrice { get; set; }
        public int WeeklyXpCost { get; set; }
        public decimal XpMultiplier { get; set; }
        public int MaxTravelPlans { get; set; }
        public bool IsActive { get; set; }
    }
}
