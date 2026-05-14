using Gamification.Application.DTOs.UserStats;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStats
{
    public record UpdateUserStatsCommand(Guid UserId, UpdateUserStatsDto Dto) : IRequest<ApiResponse<string>>;
}
