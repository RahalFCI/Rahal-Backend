namespace Rewards.Application.DTOs.TravelPlans
{
    public class CreateTravelPlanDto
    {
        public decimal BudgetLimit { get; set; }
        public int StayDurationDays { get; set; }
        public string Prompt { get; set; } = string.Empty;
    }
}
