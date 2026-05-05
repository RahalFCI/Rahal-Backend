using Gamification.Application.DTOs.UserStats;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.UserStats
{
    public record GetAllUserStatsQuery : IRequest<IEnumerable<GetUserStatsDto>>;

}
