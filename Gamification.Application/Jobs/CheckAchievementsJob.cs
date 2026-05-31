using Gamification.Application.CQRS.Queries.ExplorerAchievement;
using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gamification.Application.Jobs
{
    public class CheckAchievementsJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CheckAchievementsJob> _logger;

        public CheckAchievementsJob(IServiceScopeFactory scopeFactory, ILogger<CheckAchievementsJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var achievementRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Achievement>>();
            var explorerAchievementRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Gamification.Domain.Entities.ExplorerAchievement>>();
            var xpTransactionRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<XpTransaction>>();
            var userStatsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserStats>>();

            var userStats = await mediator.Send(new GetUserStatsByExplorerIdQuery(explorerId), cancellationToken);

            if (!userStats.IsSuccess)
            {
                _logger.LogWarning("UserStats not found for explorer {ExplorerId}", explorerId);
                return;
            }

            var statValueMap = new Dictionary<string, int>
            {
                ["TOTAL_CHECKINS"] = userStats.Data.TotalCheckIns,
                ["TOTAL_XP"] = (int)userStats.Data.CumulativeXp,
                ["LONGEST_STREAK"] = userStats.Data.LongestStreak,
                ["TOTAL_CHALLENGES"] = userStats.Data.TotalChallengesCompleted,
                ["TOTAL_BADGES"] = userStats.Data.TotalBadgesEarned,
                ["TOTAL_ACHIEVEMENTS"] = userStats.Data.TotalAchievementsEarned
            };

            var earnedAchievementIdsResult = await mediator.Send(
                new GetExplorerAchievementsByExplorerIdQuery(
                    explorerId, 
                    new OffsetPaginationRequest { Page = 1, PageSize = 1000 }), 
                cancellationToken);
            var earnedAchievementIds = earnedAchievementIdsResult.IsSuccess
                ? earnedAchievementIdsResult.Data.Items.Select(ea => ea.Id).ToList()
                : new List<Guid>();

            var candidateAchievements = await achievementRepository.GetTable()
                .Include(a => a.AchievementCriteriaType)
                .Where(a => !earnedAchievementIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

            var unlockedAchievements = candidateAchievements
                .Where(a => statValueMap.TryGetValue(a.AchievementCriteriaType?.Code ?? string.Empty, out var statValue)
                         && a.CriteriaThreshold <= statValue)
                .ToList();

            if (!unlockedAchievements.Any())
                return;

            var totalXpGained = 0;
            var badgeCount = 0;

            foreach (var achievement in unlockedAchievements)
            {
                var explorerAchievement = new Gamification.Domain.Entities.ExplorerAchievement
                {
                    AchievementId = achievement.Id,
                    ExplorerId = explorerId,
                    EarnedAt = DateTime.UtcNow,
                    IsNotified = false
                };
                explorerAchievementRepository.Add(explorerAchievement);

                var xpTransaction = new XpTransaction
                {
                    ExplorerProfileId = explorerId,
                    Amount = achievement.Xp,
                    Source = XpSourceType.Achievement,
                    ReferenceId = achievement.Id
                };
                xpTransactionRepository.Add(xpTransaction);

                totalXpGained += achievement.Xp;

                if (achievement.BadgeId != Guid.Empty)
                {
                    badgeCount++;
                }
            }

            await explorerAchievementRepository.SaveChangesAsync(cancellationToken);
            await xpTransactionRepository.SaveChangesAsync(cancellationToken);

            var newCumulativeXp = userStats.Data.CumulativeXp + totalXpGained;
            var newAvailableXp = userStats.Data.AvailableXp + totalXpGained;
            var newAchievementCount = userStats.Data.TotalAchievementsEarned + unlockedAchievements.Count;
            var newBadgeCount = userStats.Data.TotalBadgesEarned + badgeCount;

            await userStatsRepository.GetTable()
                .Where(us => us.ExplorerProfileId == explorerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(us => us.CumulativeXp, newCumulativeXp)
                    .SetProperty(us => us.AvailableXp, newAvailableXp)
                    .SetProperty(us => us.TotalAchievementCount, newAchievementCount)
                    .SetProperty(us => us.TotalBadgeCount, newBadgeCount)
                    .SetProperty(us => us.LastActivityDate, DateTime.UtcNow),
                    cancellationToken);

            await cacheService.SortedSetAddAsync("leaderboard:xp", explorerId.ToString(), (double)newCumulativeXp);

            _logger.LogInformation("Unlocked {Count} achievements for explorer {ExplorerId}", unlockedAchievements.Count, explorerId);
        }
    }
}
