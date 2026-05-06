using Gamification.Application.DTOs.AchievementCriteriaType;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.AchievementCriteriaType
{
    public record GetAchievementCriteriaTypeByNameQuery(string Name) : IRequest<ApiResponse<GetAchievementCriteriaTypeDto?>>;

}
