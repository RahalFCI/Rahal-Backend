using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class Achievement : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid BadgeId { get; set; } = Guid.Empty;
        public virtual Badge? Badge { get; set; }
        public int Xp { get; set; } = 0;
        public Guid AchievementCriteriaTypeId { get; set; } = Guid.Empty;
        public AchievementCriteriaType? AchievementCriteriaType { get; set; }
        public int CriteriaThreshold { get; set; } = 0;
        public virtual IEnumerable<ExplorerAchievement> ExplorerAchievements { get; set; } = new List<ExplorerAchievement>();
    }
}
