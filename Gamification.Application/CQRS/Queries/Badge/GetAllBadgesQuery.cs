using Gamification.Application.DTOs.Badge;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Badge
{
    public record GetAllBadgesQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetBadgeDto>>>;

}
