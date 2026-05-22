using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.UserStat
{
    public record UpdateBadgeStatsOrchestrator(Guid ExplorerId) : IRequest<ApiResponse<string>>;
}
