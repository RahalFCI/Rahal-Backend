using MediatR;
using Gamification.Application.DTOs.XpTransaction;

namespace Gamification.Application.CQRS.Queries.XpTransactions
{
    public record GetXpTransactionsByExplorerIdQuery(Guid ExplorerId) : IRequest<IEnumerable<GetXpTransactionDto>>;
}
