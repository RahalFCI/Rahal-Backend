using MediatR;
using Gamification.Application.DTOs.UserStats;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record CreateUserStatsCommand(CreateUserStatsDto Dto) : IRequest<string>;
}
