using Gamification.Application.DTOs.XpTransaction;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.XpTransactions
{
    public record CreateXpTransactionCommand(CreateXpTransactionDto Dto) : IRequest<ApiResponse<GetXpTransactionDto>>;

}
