using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.CheckInChallenge
{
    public record DeleteCheckInChallengeCommand(Guid Id) : IRequest<string>;

}
