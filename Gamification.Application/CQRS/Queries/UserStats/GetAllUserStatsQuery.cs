using Gamification.Application.DTOs.UserStats;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.UserStats
{
    public record GetAllUserStatsQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetUserStatsDto>>>;

}
