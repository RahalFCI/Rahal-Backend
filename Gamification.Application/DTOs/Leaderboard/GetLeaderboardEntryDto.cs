namespace Gamification.Application.DTOs.Leaderboard
{
    /// <summary>
    /// One row of the XP leaderboard. <see cref="ExplorerId"/> is the explorer's UserId
    /// (the gamification domain keys its per-explorer state — check-ins, stats, the
    /// leaderboard sorted set — by UserId), joined to the ExplorerProfile for display.
    /// </summary>
    public class GetLeaderboardEntryDto
    {
        public Guid ExplorerId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public int Level { get; set; }
        public long CumulativeXp { get; set; }
        /// <summary>1-based position on the leaderboard.</summary>
        public long Rank { get; set; }
    }
}
