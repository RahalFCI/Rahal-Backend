using Gamification.Application.DTOs.Challenge;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Commands.Challenge
{
    public record UpdateChallengeCommand(Guid Id, UpdateChallengeDto Dto) : IRequest<ApiResponse<string>>;

}
