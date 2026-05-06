using Gamification.Application.DTOs.Achievement;
using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Mappers
{
    public static class AchievementCriteriaTypeMapper
    {
        public static GetAchievementCriteriaTypeDto ToGetDto(AchievementCriteriaType achievement)
        {
            return new GetAchievementCriteriaTypeDto
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description
            };
        }

        public static AchievementCriteriaType ToEntity(AddAchievementCriteriaTypeDto dto)
        {
            return new AchievementCriteriaType
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        public static void UpdateEntity(AchievementCriteriaType achievement, UpdateAchievementCriteriaTypeDto dto)
        {
            achievement.Name = dto.Name;
            achievement.Description = dto.Description;
        }

        public static IEnumerable<GetAchievementCriteriaTypeDto> ToGetDtos(IEnumerable<AchievementCriteriaType?> achievements)
        {
            return achievements.Where(a => a != null).Select(a => ToGetDto(a!));
        }
    }
}
