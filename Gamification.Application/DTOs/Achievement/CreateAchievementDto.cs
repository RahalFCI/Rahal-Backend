namespace Gamification.Application.DTOs.Achievement
{
    public class CreateAchievementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid BadgeId { get; set; }
        public int XpReward { get; set; }
        public Guid CriteriaTypeId { get; set; }
        public int CriteriaThreshold { get; set; }
    }
}
