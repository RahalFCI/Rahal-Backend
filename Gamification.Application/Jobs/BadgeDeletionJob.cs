using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;

namespace Gamification.Application.Jobs
{
    public class BadgeDeletionJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BadgeDeletionJob> _logger;

        public BadgeDeletionJob(IServiceScopeFactory scopeFactory, ILogger<BadgeDeletionJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid badgeId, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var achievementRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Achievement>>();
            var explorerAchievementRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<ExplorerAchievement>>();
            var userStatsRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<UserStats>>();
            var xpTransactionRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<XpTransaction>>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            _logger.LogInformation("Starting badge deletion job for badge {BadgeId}", badgeId);

            try
            {
                var achievements = await achievementRepository.GetTable()
                    .Where(a => a.BadgeId == badgeId)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} achievements linked to badge {BadgeId}", achievements.Count, badgeId);

                foreach (var achievement in achievements)
                {
                    var explorerAchievements = await explorerAchievementRepository.GetTable()
                        .Where(ea => ea.AchievementId == achievement.Id)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("Found {Count} explorers with achievement {AchievementId}", explorerAchievements.Count, achievement.Id);

                    foreach (var explorerAchievement in explorerAchievements)
                    {
                        var xpTransaction = new XpTransaction
                        {
                            ExplorerProfileId = explorerAchievement.ExplorerId,
                            Amount = -achievement.Xp,
                            Source = XpSourceType.AchievementRevoked,
                            ReferenceId = achievement.Id
                        };

                        xpTransactionRepository.Add(xpTransaction);
                        await xpTransactionRepository.SaveChangesAsync(cancellationToken);

                        await userStatsRepository.GetTable()
                            .Where(us => us.ExplorerProfileId == explorerAchievement.ExplorerId)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(us => us.TotalAchievementCount, us => us.TotalAchievementCount - 1)
                                .SetProperty(us => us.TotalBadgeCount, us => us.TotalBadgeCount - 1)
                                .SetProperty(us => us.CumulativeXp, us => us.CumulativeXp - achievement.Xp)
                                .SetProperty(us => us.AvailableXp, us => us.AvailableXp - achievement.Xp),
                                cancellationToken);

                        var userStats = await userStatsRepository.GetTable()
                            .Where(us => us.ExplorerProfileId == explorerAchievement.ExplorerId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (userStats != null)
                        {
                            await cacheService.SortedSetAddAsync("leaderboard:xp", explorerAchievement.ExplorerId.ToString(), userStats.CumulativeXp);
                        }
                    }

                    await achievementRepository.GetTable()
                        .Where(a => a.Id == achievement.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.DeletedAt, DateTime.UtcNow)
                            .SetProperty(a => a.IsDeleted, true),
                            cancellationToken);
                }

                _logger.LogInformation("Badge deletion job completed for badge {BadgeId}", badgeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing badge deletion for badge {BadgeId}", badgeId);
                throw;
            }
        }
    }
}
