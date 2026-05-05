using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record DeleteBadgeCommand(Guid Id) : IRequest<string>;

}
