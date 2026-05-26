using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.CQRS.Orchestrators.Badges;
using Gamification.Application.Jobs;
using Gamification.Domain.Entities;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.Badges.Orchestrators
{
    public class DeleteBadgeWithAchievementReversalOrchestratorHandler : IRequestHandler<DeleteBadgeWithAchievementReversalOrchestrator, ApiResponse<string>>
    {
        private readonly IGenericRepository<Badge> _badgeRepository;
        private readonly IGenericRepository<Achievement> _achievementRepository;
        private readonly IMediator _mediator;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DeleteBadgeWithAchievementReversalOrchestratorHandler> _logger;

        public DeleteBadgeWithAchievementReversalOrchestratorHandler(
            IGenericRepository<Badge> badgeRepository,
            IGenericRepository<Achievement> achievementRepository,
            IMediator mediator,
            IBackgroundJobClient backgroundJobClient,
            ILogger<DeleteBadgeWithAchievementReversalOrchestratorHandler> logger)
        {
            _badgeRepository = badgeRepository;
            _achievementRepository = achievementRepository;
            _mediator = mediator;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteBadgeWithAchievementReversalOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting badge deletion with achievement reversal for badge {BadgeId}", request.BadgeId);

            var badge = await _badgeRepository.GetTable()
                .Where(b => b.Id == request.BadgeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (badge == null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.BadgeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var linkedAchievements = await _achievementRepository.GetTable()
                .Where(a => a.BadgeId == request.BadgeId)
                .CountAsync(cancellationToken);

            _logger.LogInformation("Found {Count} achievements linked to badge {BadgeId}", linkedAchievements, request.BadgeId);

            _backgroundJobClient.Enqueue<BadgeDeletionJob>(j => j.ExecuteAsync(request.BadgeId, CancellationToken.None));

            var deleteResult = await _mediator.Send(new DeleteBadgeCommand(request.BadgeId), cancellationToken);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogError("Failed to delete badge {BadgeId}. Error: {ErrorCode}", request.BadgeId, deleteResult.errorCode);
                return deleteResult;
            }

            _logger.LogInformation("Badge {BadgeId} deleted successfully. Achievement reversal job enqueued", request.BadgeId);
            return ApiResponse<string>.Success("Badge deleted successfully");
        }
    }
}
