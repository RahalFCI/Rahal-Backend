using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.AchievementCriteriaTypes
{
    public record GetAllAchievementCriteriaTypesQuery : IRequest<ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>>;
}
