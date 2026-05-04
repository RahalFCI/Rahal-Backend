namespace Gamification.Application.DTOs.UserStats
{
    public class GetUserStatsDto
    {
        public Guid Id { get; set; }
        public Guid ExplorerId { get; set; }
        public int TotalCheckIns { get; set; }
        public int TotalChallengesCompleted { get; set; }
        public int TotalAchievementsEarned { get; set; }
        public int TotalBadgesEarned { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public int TotalXpEarned { get; set; }
    }
}
