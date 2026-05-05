using Gamification.Application.DTOs.XpTransaction;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.XpTransactions
{
    public record CreateXpTransactionCommand(CreateXpTransactionDto Dto) : IRequest<string>;

}
