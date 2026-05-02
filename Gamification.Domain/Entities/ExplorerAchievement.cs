using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class ExplorerAchievement : BaseEntity
    {
        public Guid AchievementId { get; set; } = Guid.Empty;
        public Achievement? Achievement { get; set; }
        public Guid ExplorerProfileId { get; set; } = Guid.Empty;

    }
}
