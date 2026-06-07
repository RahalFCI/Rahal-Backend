namespace Gamification.Application.DTOs.Achievement
{
    public class GetAchievementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? BadgeId { get; set; }
        public string BadgeName { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public Guid CriteriaTypeId { get; set; }
        public string CriteriaCode { get; set; } = string.Empty;
        public int CriteriaThreshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
