using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.UserStat
{
    public record UpdateCheckInStatsOrchestrator(Guid ExplorerId, Guid CheckInId, int XpAmount) : IRequest<ApiResponse<string>>;
}
