using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.Challenges
{
    public record DeleteChallengeWithXpReversalOrchestrator(Guid ChallengeId) : IRequest<ApiResponse<string>>;
}
