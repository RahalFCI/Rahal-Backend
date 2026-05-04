using MediatR;
using Gamification.Application.DTOs.Challenge;

namespace Gamification.Application.CQRS.Commands.Challenges
{
    public record CreateChallengeCommand(CreateChallengeDto Dto) : IRequest<string>;
    public record UpdateChallengeCommand(Guid Id, UpdateChallengeDto Dto) : IRequest<string>;
    public record DeleteChallengeCommand(Guid Id) : IRequest<string>;
}
