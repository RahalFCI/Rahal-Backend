using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Orchestrators
{
    public class DeleteExplorerProfileWithUserStatsOrchestratorHandler : IRequestHandler<DeleteExplorerProfileWithUserStatsOrchestrator, ApiResponse<string>>
    {
        private readonly IMediator _mediator;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteExplorerProfileWithUserStatsOrchestratorHandler> _logger;

        public DeleteExplorerProfileWithUserStatsOrchestratorHandler(IMediator mediator, IGamificationUnitOfWork unitOfWork, ILogger<DeleteExplorerProfileWithUserStatsOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteExplorerProfileWithUserStatsOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile deletion orchestration for user {UserId}", request.Id);
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                //Query explorer profile to get profile picture URL before deletion
                var explorerProfileResult = await _mediator.Send(new GetExplorerProfileByIdQuery(request.Id), cancellationToken);
                if(!explorerProfileResult.IsSuccess)
                {
                    _logger.LogError("Explorer profile not found for user {UserId}", request.Id);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(explorerProfileResult.errorCode);
                }

                //Delete Explorer Profile
                var profileResult = await _mediator.Send(new DeleteExplorerProfileCommand(request.Id), cancellationToken);
                if (!profileResult.IsSuccess)
                {
                    _logger.LogError("Failed to delete explorer profile for user {UserId} with error code {ErrorCode}", request.Id, profileResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(profileResult.errorCode);
                }

                //Delete user stats
                var userStatsResult = await _mediator.Send(new DeleteUserStatsCommand(explorerProfileResult.Data.UserId), cancellationToken);
                if (!userStatsResult.IsSuccess)
                {
                    _logger.LogError("Failed to delete user stats for explorer {ExplorerId} with error code {ErrorCode}", explorerProfileResult.Data.UserId, userStatsResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(userStatsResult.errorCode);
                }

                _logger.LogError("Profile deletion orchestration completed for explorer profile for user {UserId}", request.Id);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return ApiResponse<string>.Success(profileResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting explorer profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
