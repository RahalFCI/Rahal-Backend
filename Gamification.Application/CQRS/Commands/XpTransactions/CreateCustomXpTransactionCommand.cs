using Gamification.Application.DTOs.XpTransaction;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.XpTransactions
{
    public record CreateCustomXpTransactionCommand(Guid ExplorerId, int XpAmount, string SourceType, Guid ReferenceId) : IRequest<ApiResponse<GetXpTransactionDto>>;

}
