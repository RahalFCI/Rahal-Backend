namespace Gamification.Application.DTOs.Leaderboard
{
    /// <summary>
    /// The XP leaderboard: the top explorers plus the caller's own standing
    /// (<see cref="Me"/>), which may be null when the caller has no ranked XP yet,
    /// and may also appear within <see cref="Entries"/> when the caller is in the top N.
    /// </summary>
    public class GetLeaderboardDto
    {
        public List<GetLeaderboardEntryDto> Entries { get; set; } = new();
        public GetLeaderboardEntryDto? Me { get; set; }
    }
}
