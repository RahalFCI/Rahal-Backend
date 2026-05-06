using Gamification.Application.DTOs.Achievement;
using Gamification.Application.DTOs.AchievementCriteriaType;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.AchievementCriteriaTypes
{
    public record GetAchievementCriteriaTypeByIdQuery(Guid Id) : IRequest<ApiResponse<GetAchievementCriteriaTypeDto?>>;
}
