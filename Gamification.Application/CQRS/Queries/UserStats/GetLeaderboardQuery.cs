using Gamification.Application.DTOs.Leaderboard;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Queries.UserStats
{
    /// <summary>
    /// Reads the top-N XP leaderboard from the Redis sorted set and resolves the
    /// caller's own rank. <paramref name="CurrentUserId"/> is the authenticated user's id.
    /// </summary>
    public record GetLeaderboardQuery(Guid CurrentUserId, int Count = 10)
        : IRequest<ApiResponse<GetLeaderboardDto>>;
}
