using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.CQRS.Orchestrators.Achievements;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Achievements.Orchestrators
{
    public class DeleteAchievementWithXpReversalOrchestratorHandler : IRequestHandler<DeleteAchievementWithXpReversalOrchestrator, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Achievement> _achievementRepository;
        private readonly IGamificationRepository<Gamification.Domain.Entities.ExplorerAchievement> _explorerAchievementRepository;
        private readonly IGamificationRepository<XpTransaction> _xpTransactionRepository;
        private readonly IGamificationRepository<UserStats> _userStatsRepository;
        private readonly ICacheService _cacheService;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteAchievementWithXpReversalOrchestratorHandler> _logger;

        public DeleteAchievementWithXpReversalOrchestratorHandler(
            IGamificationRepository<Achievement> achievementRepository,
            IGamificationRepository<Gamification.Domain.Entities.ExplorerAchievement> explorerAchievementRepository,
            IGamificationRepository<XpTransaction> xpTransactionRepository,
            IGamificationRepository<UserStats> userStatsRepository,
            ICacheService cacheService,
            IMediator mediator,
            ILogger<DeleteAchievementWithXpReversalOrchestratorHandler> logger)
        {
            _achievementRepository = achievementRepository;
            _explorerAchievementRepository = explorerAchievementRepository;
            _xpTransactionRepository = xpTransactionRepository;
            _userStatsRepository = userStatsRepository;
            _cacheService = cacheService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteAchievementWithXpReversalOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting achievement deletion with XP reversal for achievement {AchievementId}", request.AchievementId);

            var achievement = await _achievementRepository.GetTable()
                .Where(a => a.Id == request.AchievementId)
                .FirstOrDefaultAsync(cancellationToken);

            if (achievement == null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.AchievementId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var xpReward = achievement.Xp;

            var deleteResult = await _mediator.Send(new DeleteAchievementCommand(request.AchievementId), cancellationToken);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogError("Failed to delete achievement {AchievementId}. Error: {ErrorCode}", request.AchievementId, deleteResult.errorCode);
                return deleteResult;
            }

            var explorerAchievements = await _explorerAchievementRepository.GetTable()
                .Where(ea => ea.AchievementId == request.AchievementId)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} explorers with achievement {AchievementId}", explorerAchievements.Count, request.AchievementId);

            foreach (var explorerAchievement in explorerAchievements)
            {
                var xpTransaction = new XpTransaction
                {
                    ExplorerProfileId = explorerAchievement.ExplorerId,
                    Amount = -xpReward,
                    Source = XpSourceType.AchievementRevoked,
                    ReferenceId = request.AchievementId
                };

                _xpTransactionRepository.Add(xpTransaction);
                await _xpTransactionRepository.SaveChangesAsync(cancellationToken);

                await _userStatsRepository.GetTable()
                    .Where(us => us.ExplorerProfileId == explorerAchievement.ExplorerId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(us => us.TotalAchievementCount, us => us.TotalAchievementCount - 1)
                        .SetProperty(us => us.CumulativeXp, us => us.CumulativeXp - xpReward)
                        .SetProperty(us => us.AvailableXp, us => us.AvailableXp - xpReward),
                        cancellationToken);

                var updatedUserStats = await _userStatsRepository.GetTable()
                    .Where(us => us.ExplorerProfileId == explorerAchievement.ExplorerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (updatedUserStats != null)
                {
                    await _cacheService.SortedSetAddAsync("leaderboard:xp", explorerAchievement.ExplorerId.ToString(), updatedUserStats.CumulativeXp);
                }
            }

            _logger.LogInformation("Achievement {AchievementId} deleted successfully. XP reversal completed for {Count} explorers", 
                request.AchievementId, explorerAchievements.Count);
            return ApiResponse<string>.Success("Achievement deleted successfully");
        }
    }
}
