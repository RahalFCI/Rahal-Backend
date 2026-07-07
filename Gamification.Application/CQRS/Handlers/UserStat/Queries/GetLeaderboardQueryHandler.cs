using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.Leaderboard;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.UserStat.Queries
{
    /// <summary>
    /// Surfaces the leaderboard that the gamification jobs already maintain: a Redis
    /// sorted set (<c>leaderboard:xp</c>) of explorers by cumulative XP. This handler is
    /// the read door over the existing <see cref="ICacheService"/> primitives — it reads
    /// the top N members (keyed by UserId) and joins the ExplorerProfile for display,
    /// then resolves the caller's own rank so a user outside the top N still sees theirs.
    /// </summary>
    public class GetLeaderboardQueryHandler
        : IRequestHandler<GetLeaderboardQuery, ApiResponse<GetLeaderboardDto>>
    {
        private const string LeaderboardKey = "leaderboard:xp";

        private readonly ICacheService _cacheService;
        private readonly IGamificationRepository<ExplorerProfile> _profiles;

        public GetLeaderboardQueryHandler(
            ICacheService cacheService,
            IGamificationRepository<ExplorerProfile> profiles)
        {
            _cacheService = cacheService;
            _profiles = profiles;
        }

        public async Task<ApiResponse<GetLeaderboardDto>> Handle(
            GetLeaderboardQuery request,
            CancellationToken cancellationToken)
        {
            var count = request.Count <= 0 ? 10 : request.Count;
            var top = await _cacheService.SortedSetGetTopAsync(LeaderboardKey, count);

            // Members are UserId strings; collect them plus the caller so we can join
            // display data in a single query.
            var memberIds = top
                .Select(t => Guid.TryParse(t.Member, out var id) ? id : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var idsToLoad = memberIds.Append(request.CurrentUserId).Distinct().ToList();

            var profiles = await _profiles.GetTable()
                .Where(p => idsToLoad.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, cancellationToken);

            GetLeaderboardEntryDto ToEntry(Guid explorerId, double score, long rank)
            {
                profiles.TryGetValue(explorerId, out var profile);
                return new GetLeaderboardEntryDto
                {
                    ExplorerId = explorerId,
                    DisplayName = profile?.DisplayName ?? string.Empty,
                    ProfilePictureUrl = profile?.ProfilePictureURL ?? string.Empty,
                    Level = profile?.Level ?? 1,
                    CumulativeXp = (long)score,
                    Rank = rank,
                };
            }

            var entries = new List<GetLeaderboardEntryDto>();
            for (var i = 0; i < top.Count; i++)
            {
                if (!Guid.TryParse(top[i].Member, out var explorerId)) continue;
                entries.Add(ToEntry(explorerId, top[i].Score, i + 1));
            }

            // The caller's own standing — from the top slice if present, else a direct
            // rank/score lookup so a user outside the top N still gets their number.
            GetLeaderboardEntryDto? me = entries.FirstOrDefault(e => e.ExplorerId == request.CurrentUserId);
            if (me is null)
            {
                var myMember = request.CurrentUserId.ToString();
                var myRank = await _cacheService.SortedSetGetRankAsync(LeaderboardKey, myMember);
                var myScore = await _cacheService.SortedSetGetScoreAsync(LeaderboardKey, myMember);
                if (myRank.HasValue && myScore.HasValue)
                {
                    me = ToEntry(request.CurrentUserId, myScore.Value, myRank.Value + 1);
                }
            }

            return ApiResponse<GetLeaderboardDto>.Success(new GetLeaderboardDto
            {
                Entries = entries,
                Me = me,
            });
        }
    }
}
