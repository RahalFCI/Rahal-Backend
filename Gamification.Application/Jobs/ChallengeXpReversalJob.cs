using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Gamification.Application.Interfaces;

namespace Gamification.Application.Jobs
{
    public class ChallengeXpReversalJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChallengeXpReversalJob> _logger;

        public ChallengeXpReversalJob(IServiceScopeFactory scopeFactory, ILogger<ChallengeXpReversalJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid challengeId, int xpReward, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var checkInChallengeRepository = scope.ServiceProvider.GetRequiredService<IGamificationRepository<CheckInChallenge>>();
            var userStatsRepository = scope.ServiceProvider.GetRequiredService<IGamificationRepository<UserStats>>();
            var xpTransactionRepository = scope.ServiceProvider.GetRequiredService<IGamificationRepository<XpTransaction>>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            _logger.LogInformation("Starting XP reversal for deleted challenge {ChallengeId}", challengeId);

            try
            {
                var checkInChallenges = await checkInChallengeRepository.GetTable()
                    .Where(cc => cc.ChallengeId == challengeId)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} check-in challenges for challenge {ChallengeId}", checkInChallenges.Count, challengeId);

                var explorerIds = new HashSet<Guid>();
                foreach (var checkInChallenge in checkInChallenges)
                {
                    explorerIds.Add(checkInChallenge.ExplorerId);
                }

                foreach (var explorerId in explorerIds)
                {
                    var xpTransaction = new XpTransaction
                    {
                        ExplorerProfileId = explorerId,
                        Amount = -xpReward,
                        Source = XpSourceType.ChallengeRevoked,
                        ReferenceId = challengeId
                    };

                    xpTransactionRepository.Add(xpTransaction);
                    await xpTransactionRepository.SaveChangesAsync(cancellationToken);

                    await userStatsRepository.GetTable()
                        .Where(us => us.ExplorerProfileId == explorerId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(us => us.TotalChallengeCount, us => us.TotalChallengeCount - 1)
                            .SetProperty(us => us.CumulativeXp, us => us.CumulativeXp - xpReward)
                            .SetProperty(us => us.AvailableXp, us => us.AvailableXp - xpReward),
                            cancellationToken);

                    var userStats = await userStatsRepository.GetTable()
                        .Where(us => us.ExplorerProfileId == explorerId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (userStats != null)
                    {
                        await cacheService.SortedSetAddAsync("leaderboard:xp", explorerId.ToString(), userStats.CumulativeXp);
                    }
                }

                _logger.LogInformation("XP reversal completed for challenge {ChallengeId}", challengeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing XP reversal for challenge {ChallengeId}", challengeId);
                throw;
            }
        }
    }
}
