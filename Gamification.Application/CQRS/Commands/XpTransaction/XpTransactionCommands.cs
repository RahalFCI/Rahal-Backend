using MediatR;
using Gamification.Application.DTOs.XpTransaction;

namespace Gamification.Application.CQRS.Commands.XpTransactions
{
    public record CreateXpTransactionCommand(CreateXpTransactionDto Dto) : IRequest<string>;
}
