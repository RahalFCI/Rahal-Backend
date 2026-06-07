using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Events.CheckIns;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.EventConsumers
{
    public class CheckInInvalidatedEventConsumer : IConsumer<CheckInInvalidatedEvent>
    {
        private readonly IGamificationRepository<UserStats> _userStatsRepository;
        private readonly IGamificationRepository<XpTransaction> _xpTransactionRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CheckInInvalidatedEventConsumer> _logger;

        public CheckInInvalidatedEventConsumer(
            IGamificationRepository<UserStats> userStatsRepository,
            IGamificationRepository<XpTransaction> xpTransactionRepository,
            ICacheService cacheService,
            ILogger<CheckInInvalidatedEventConsumer> logger)
        {
            _userStatsRepository = userStatsRepository;
            _xpTransactionRepository = xpTransactionRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CheckInInvalidatedEvent> context)
        {
            var cancellationToken = context.CancellationToken;
            var explorerId = context.Message.ExplorerId;
            var checkInId = context.Message.CheckInId;
            var xpReward = context.Message.XpReward;

            _logger.LogInformation("Processing check-in invalidation event for explorer {ExplorerId} with check-in {CheckInId}", explorerId, checkInId);

            try
            {
                var xpTransaction = new XpTransaction
                {
                    ExplorerProfileId = explorerId,
                    Amount = -xpReward,
                    Source = XpSourceType.CheckInInvalidated,
                    ReferenceId = checkInId
                };

                _xpTransactionRepository.Add(xpTransaction);
                await _xpTransactionRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("XP transaction created for explorer {ExplorerId} with check-in {CheckInId}", explorerId, checkInId);

                await _userStatsRepository.GetTable()
                    .Where(us => us.ExplorerProfileId == explorerId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(us => us.TotalCheckInCount, us => us.TotalCheckInCount - 1)
                        .SetProperty(us => us.CumulativeXp, us => us.CumulativeXp - xpReward)
                        .SetProperty(us => us.AvailableXp, us => us.AvailableXp - xpReward),
                        cancellationToken);

                _logger.LogInformation("User stats updated for explorer {ExplorerId}", explorerId);

                var userStats = await _userStatsRepository.GetTable()
                    .Where(us => us.ExplorerProfileId == explorerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (userStats != null)
                {
                    await _cacheService.SortedSetAddAsync("leaderboard:xp", explorerId.ToString(), userStats.CumulativeXp);
                }

                _logger.LogInformation("Check-in invalidation event processed successfully for explorer {ExplorerId}", explorerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing check-in invalidation event for explorer {ExplorerId}. Message will be retried", explorerId);
                throw;
            }
        }
    }
}
