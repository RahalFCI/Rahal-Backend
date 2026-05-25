using Shared.Domain.Entities;

namespace Gamification.Domain.Entities
{
    public class ExplorerAchievement : BaseEntity
    {
        public Guid AchievementId { get; set; }
        public virtual Achievement? Achievement { get; set; }
        public Guid ExplorerId { get; set; }
        public ExplorerProfile? ExplorerProfile { get; set; }
        public DateTime EarnedAt { get; set; }
        public bool IsNotified { get; set; }
    }
}
