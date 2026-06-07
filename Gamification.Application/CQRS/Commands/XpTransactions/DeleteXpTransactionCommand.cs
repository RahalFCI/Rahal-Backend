using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.XpTransactions
{
    public record DeleteXpTransactionCommand(Guid Id, Guid ExplorerId, int ExistingXp) : IRequest<ApiResponse<string>>;


}
