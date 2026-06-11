namespace Rewards.Application.DTOs.TravelPlans
{
    public class GetTravelPlanDto
    {
        public Guid Id { get; set; }
        public Guid ExplorerId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal BudgetLimit { get; set; }
        public int StayDurationDays { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string GeneratedPlanJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; }
    }
}
