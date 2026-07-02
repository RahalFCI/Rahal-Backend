using Gamification.Application.DTOs.UserStats;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class UserStatsMapper
    {
        public static GetUserStatsDto ToGetDto(Domain.Entities.UserStats userStats)
            {
                return new GetUserStatsDto
                {
                    Id = userStats.Id,
                    ExplorerId = userStats.ExplorerProfileId,
                    ExplorerName = userStats.ExplorerProfile?.DisplayName ?? string.Empty,
                    AvailableXp = userStats.AvailableXp,
                    CumulativeXp = userStats.CumulativeXp,
                    CurrentStreak = userStats.CurrentStreak,
                    LastActivityDate = userStats.LastActivityDate,
                    TotalCheckIns = userStats.TotalCheckInCount,
                    TotalChallengesCompleted = userStats.TotalChallengeCount,
                    TotalAchievementsEarned = userStats.TotalAchievementCount,
                    TotalBadgesEarned = userStats.TotalBadgeCount,
                    LongestStreak = userStats.LongestStreak
                };
            }

            public static Domain.Entities.UserStats ToEntity(CreateUserStatsDto dto)
            {
                return new Domain.Entities.UserStats
                {
                    ExplorerProfileId = dto.ExplorerId,
                    AvailableXp = 0,
                    CumulativeXp = 0,
                    CurrentStreak = 0,
                    LastActivityDate = null,
                    TotalCheckInCount = 0,
                    TotalChallengeCount = 0,
                    TotalAchievementCount = 0,
                    TotalBadgeCount = 0,
                    LongestStreak = 0
                };
            }

        public static IEnumerable<GetUserStatsDto> ToGetDtos(IEnumerable<Domain.Entities.UserStats?> statsList)
        {
            return statsList.Where(s => s != null).Select(s => ToGetDto(s!));
        }
    }
}
