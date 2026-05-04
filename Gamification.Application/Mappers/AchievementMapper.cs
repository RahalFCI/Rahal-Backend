using Gamification.Application.DTOs.Achievement;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class AchievementMapper
    {
        public static GetAchievementDto ToGetDto(Achievement achievement)
        {
            return new GetAchievementDto
            {
                Id = achievement.Id,
                Title = achievement.Title,
                Description = achievement.Description,
                BadgeId = achievement.BadgeId,
                BadgeName = achievement.Badge?.Name ?? string.Empty,
                XpReward = achievement.Xp,
                CriteriaTypeId = achievement.AchievementCriteriaTypeId,
                CriteriaCode = achievement.AchievementCriteriaType?.Name ?? string.Empty,
                CriteriaThreshold = achievement.CriteriaThreshold,
                CreatedAt = achievement.CreatedAt,
                UpdatedAt = achievement.UpdatedAt
            };
        }

        public static Achievement ToEntity(CreateAchievementDto dto)
        {
            return new Achievement
            {
                Title = dto.Title,
                Description = dto.Description,
                BadgeId = dto.BadgeId,
                Xp = dto.XpReward,
                AchievementCriteriaTypeId = dto.CriteriaTypeId,
                CriteriaThreshold = dto.CriteriaThreshold
            };
        }

        public static void UpdateEntity(Achievement achievement, UpdateAchievementDto dto)
        {
            achievement.Title = dto.Title;
            achievement.Description = dto.Description;
            achievement.BadgeId = dto.BadgeId;
            achievement.Xp = dto.XpReward;
            achievement.AchievementCriteriaTypeId = dto.CriteriaTypeId;
            achievement.CriteriaThreshold = dto.CriteriaThreshold;
        }

        public static IEnumerable<GetAchievementDto> ToGetDtos(IEnumerable<Achievement?> achievements)
        {
            return achievements.Where(a => a != null).Select(a => ToGetDto(a!));
        }
    }
}
