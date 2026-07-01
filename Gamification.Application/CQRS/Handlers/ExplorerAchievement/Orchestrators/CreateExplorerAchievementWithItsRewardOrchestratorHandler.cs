using Gamification.Application.CQRS.Commands.ExplorerAchievement;
using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Orchestrators.ExplorerAchievements;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Application.CQRS.Queries.Achievement;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Orchestrators
{
    public class CreateExplorerAchievementWithItsRewardOrchestratorHandler : IRequestHandler<CreateExplorerAchievementWithItsRewardOrchestrator, ApiResponse<string>>
    {
        private readonly IMediator _mediator;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly ILogger<CreateExplorerAchievementWithItsRewardOrchestratorHandler> _logger;

        public CreateExplorerAchievementWithItsRewardOrchestratorHandler(
            IMediator mediator,
            IGamificationUnitOfWork unitOfWork,
            ILogger<CreateExplorerAchievementWithItsRewardOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateExplorerAchievementWithItsRewardOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating explorer achievement with its reward for explorer {ExplorerId} and achievement {AchievementId}", request.Dto.ExplorerId, request.Dto.AchievementId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // retrieve achievement
            var achievementResult = await _mediator.Send(new GetAchievementByIdQuery(request.Dto.AchievementId), cancellationToken);
            if(!achievementResult.IsSuccess)
            {
                _logger.LogError("Failed to retrieve achievement with id {AchievementId} for explorer {ExplorerId}", request.Dto.AchievementId, request.Dto.ExplorerId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<string>.Failure(achievementResult.errorCode);
            }

            // create explorer achievement
            var explorerAchievementResult = await _mediator.Send(new CreateExplorerAchievementCommand(request.Dto), cancellationToken);
            if (!explorerAchievementResult.IsSuccess)
            {
                _logger.LogError("Failed to create explorer achievement for explorer {ExplorerId} and achievement {AchievementId}", request.Dto.ExplorerId, request.Dto.AchievementId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<string>.Failure(explorerAchievementResult.errorCode);
            }

            //Update Explorer achievement stats
            var AchievementRewardResult = await _mediator.Send(new UpdateAchievementStatsOrchestrator(request.Dto.ExplorerId, achievementResult.Data.XpReward), cancellationToken);
            if (!AchievementRewardResult.IsSuccess)
            {
                _logger.LogError("Failed to update achievement stats for explorer {ExplorerId} and achievement {AchievementId}", request.Dto.ExplorerId, request.Dto.AchievementId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<string>.Failure(AchievementRewardResult.errorCode);
            }


            // create xp reward
            if (achievementResult.Data.XpReward > 0)
            {
                var createXpRewardResult = await _mediator.Send(new CreateXpTransactionCommand(new CreateXpTransactionDto
                {
                    ExplorerId = request.Dto.ExplorerId,
                    ReferenceId = request.Dto.AchievementId,
                    SourceType = XpSourceType.Achievement.ToString(),
                }));
                if (!createXpRewardResult.IsSuccess)
                {
                    _logger.LogError("Failed to create xp reward for explorer {ExplorerId} and achievement {AchievementId}", request.Dto.ExplorerId, request.Dto.AchievementId);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(createXpRewardResult.errorCode);
                }
            }

            //Create badge reward
            if(achievementResult.Data.BadgeId is not null)
            {
                var BadgeRewardResult = await _mediator.Send(new UpdateBadgeStatsOrchestrator(request.Dto.ExplorerId), cancellationToken);
                if(!BadgeRewardResult.IsSuccess)
                {
                    _logger.LogError("Failed to create badge reward for explorer {ExplorerId} and achievement {AchievementId}", request.Dto.ExplorerId, request.Dto.AchievementId);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(BadgeRewardResult.errorCode);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApiResponse<string>.Success("Explorer achievement created with its rewards successfully");


        }
    }
}
