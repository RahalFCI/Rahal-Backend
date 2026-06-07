using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record DecrementBadgeStatsCommand(Guid ExplorerId) : IRequest<ApiResponse<string>>;
}
