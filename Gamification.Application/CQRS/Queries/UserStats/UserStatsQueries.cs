using MediatR;
using Gamification.Application.DTOs.UserStats;

namespace Gamification.Application.CQRS.Queries.UserStat
{
    public record GetUserStatsByExplorerIdQuery(Guid ExplorerId) : IRequest<GetUserStatsDto?>;
    public record GetAllUserStatsQuery : IRequest<IEnumerable<GetUserStatsDto>>;
}
