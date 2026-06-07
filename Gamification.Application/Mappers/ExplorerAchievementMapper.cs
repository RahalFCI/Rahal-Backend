using Gamification.Application.DTOs.ExplorerAchievement;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class ExplorerAchievementMapper
    {
        public static GetExplorerAchievementDto ToGetDto(ExplorerAchievement explorerAchievement)
        {
            return new GetExplorerAchievementDto
            {
                Id = explorerAchievement.Id,
                AchievementId = explorerAchievement.AchievementId,
                AchievementTitle = explorerAchievement.Achievement?.Title ?? string.Empty,
                ExplorerId = explorerAchievement.ExplorerId,
                EarnedAt = explorerAchievement.EarnedAt,
                IsNotified = explorerAchievement.IsNotified
            };
        }

        public static ExplorerAchievement ToEntity(CreateExplorerAchievementDto dto)
        {
            return new ExplorerAchievement
            {
                AchievementId = dto.AchievementId,
                ExplorerId = dto.ExplorerId,
                EarnedAt = DateTime.UtcNow,
                IsNotified = false
            };
        }

        public static IEnumerable<GetExplorerAchievementDto> ToGetDtos(IEnumerable<ExplorerAchievement> explorerAchievements)
        {
            return explorerAchievements.Select(ToGetDto);
        }
    }
}
