using Gamification.Application.DTOs.XpTransaction;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.XpTransactions
{
    public record GetXpTransactionsByExplorerIdQuery(Guid ExplorerId) : IRequest<ApiResponse<IEnumerable<GetXpTransactionDto>>>;

}
