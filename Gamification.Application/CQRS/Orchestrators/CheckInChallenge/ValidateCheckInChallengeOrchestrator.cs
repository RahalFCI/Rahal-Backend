using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.CheckInChallenge
{
    public record ValidateCheckInChallengeOrchestrator(Guid Id) : IRequest<ApiResponse<bool>>;
}
