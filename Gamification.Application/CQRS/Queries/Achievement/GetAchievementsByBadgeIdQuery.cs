using Gamification.Application.DTOs.Achievement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Achievement
{
    public record GetAchievementsByBadgeIdQuery(Guid BadgeId) : IRequest<IEnumerable<GetAchievementDto>>;

}
