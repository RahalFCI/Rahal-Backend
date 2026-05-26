using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.CheckInChallenges
{
    public record CreateCheckInChallengeCommand(CreateCheckInChallengeDto Dto) : IRequest<ApiResponse<GetCheckInChallengeDto>>;
}
