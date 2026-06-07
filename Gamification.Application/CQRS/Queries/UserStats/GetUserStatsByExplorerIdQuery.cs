using Gamification.Application.DTOs.UserStats;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.UserStats
{
    public record GetUserStatsByExplorerIdQuery(Guid ExplorerId) : IRequest<ApiResponse<GetUserStatsDto>>;

}
