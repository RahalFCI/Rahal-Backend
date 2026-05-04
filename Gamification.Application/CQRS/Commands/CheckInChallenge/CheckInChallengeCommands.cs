using MediatR;
using Gamification.Application.DTOs.CheckInChallenge;

namespace Gamification.Application.CQRS.Commands.CheckInChallenges
{
    public record CreateCheckInChallengeCommand(CreateCheckInChallengeDto Dto) : IRequest<string>;
    public record DeleteCheckInChallengeCommand(Guid Id) : IRequest<string>;
}
