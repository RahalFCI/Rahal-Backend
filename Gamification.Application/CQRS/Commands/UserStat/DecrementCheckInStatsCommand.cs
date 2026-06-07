using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record DecrementCheckInStatsCommand(Guid ExplorerId) : IRequest<ApiResponse<string>>;
}
