using Gamification.Application.DTOs.UserStats;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record CreateUserStatsCommand(CreateUserStatsDto Dto) : IRequest<ApiResponse<GetUserStatsDto>>;

}
