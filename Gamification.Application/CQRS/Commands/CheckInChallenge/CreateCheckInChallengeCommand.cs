using Gamification.Application.DTOs.CheckInChallenge;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.CheckInChallenge
{
    public record CreateCheckInChallengeCommand(CreateCheckInChallengeDto Dto) : IRequest<string>;

}
