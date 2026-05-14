using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.UserStats
{
    public class UpdateUserStatsDto
    {
        public int TotalCheckIns { get; set; }
        public int TotalChallengesCompleted { get; set; }
        public int TotalAchievementsEarned { get; set; }
        public int TotalBadgesEarned { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int TotalXpEarned { get; set; }
    }
}
