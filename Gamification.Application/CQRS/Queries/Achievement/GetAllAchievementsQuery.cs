using Gamification.Application.DTOs.Achievement;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Achievement
{
    public record GetAllAchievementsQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetAchievementDto>>>;

}
