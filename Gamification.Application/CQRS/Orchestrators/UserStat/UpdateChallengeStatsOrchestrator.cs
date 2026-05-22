using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.UserStat
{
    public record UpdateChallengeStatsOrchestrator(Guid ExplorerId, int Xp) : IRequest<ApiResponse<string>>;
}
