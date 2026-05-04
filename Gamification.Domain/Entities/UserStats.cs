using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class UserStats : BaseEntity
    {
        public Guid ExplorerProfileId { get; set; } = Guid.Empty;
        //TODO: add navigation property
        public int TotalCheckInCount { get; set; } = 0;
        public int TotalAchievementCount { get; set; } = 0;
        public int TotalChallengeCount { get; set; } = 0;
        public int TotalBadgeCount { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;

    }
}
