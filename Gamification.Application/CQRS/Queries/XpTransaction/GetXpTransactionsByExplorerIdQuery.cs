using Gamification.Application.DTOs.XpTransaction;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.XpTransactions
{
    public record GetXpTransactionsByExplorerIdQuery(Guid ExplorerId, OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetXpTransactionDto>>>;

}
