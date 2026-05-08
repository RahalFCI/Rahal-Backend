using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.CheckInChallenge
{
    public record ValidateCheckInChallengeCommand(Guid Id) : IRequest<ApiResponse<bool>>;
}
