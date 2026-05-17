using Gamification.Application.DTOs.Explorer;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetExplorerProfilesIncludingDeletedQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetExplorerDto>>>;
}
