using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.AchievementCriteriaType
{
    public record GetAchievementCriteriaTypeDto(Guid Id, string Name, string Description)
    {
        public GetAchievementCriteriaTypeDto() : this(Guid.Empty, string.Empty, string.Empty)
        {
        }
    }
}
