using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Challenge
{
    public record DeleteChallengeCommand(Guid Id) : IRequest<string>;

}
