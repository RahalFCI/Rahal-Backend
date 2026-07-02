namespace Gamification.Application.DTOs.ExplorerAchievement
{
    public class GetExplorerAchievementDto
    {
        public Guid Id { get; set; }
        public Guid AchievementId { get; set; }
        public string AchievementTitle { get; set; } = string.Empty;
        public Guid ExplorerId { get; set; }
        public string ExplorerName { get; set; } = string.Empty;
        public DateTime EarnedAt { get; set; }
        public bool IsNotified { get; set; }
    }
}
