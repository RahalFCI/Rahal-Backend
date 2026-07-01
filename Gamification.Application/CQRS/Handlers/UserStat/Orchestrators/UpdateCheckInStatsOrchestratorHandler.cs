using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Commands.XpTransactions;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Application.DTOs.XpTransaction;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using Gamification.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.UserStat.Orchestrators
{
    public class UpdateCheckInStatsOrchestratorHandler : IRequestHandler<UpdateCheckInStatsOrchestrator, ApiResponse<string>>
    {
        private readonly IGamificationRepository<UserStats> _repository;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateCheckInStatsOrchestratorHandler> _logger;

        public UpdateCheckInStatsOrchestratorHandler(
            IGamificationRepository<UserStats> repository,
            IGamificationUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<UpdateCheckInStatsOrchestratorHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateCheckInStatsOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {

                _logger.LogInformation("Incrementing check-in count for explorer {ExplorerId}", request.ExplorerId);

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Retrieve user stats
                var userStats = await _repository.GetTable()
                    .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

                if (userStats is null)
                {
                    _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                //Update User Stats
                userStats.TotalCheckInCount += 1;
                userStats.AvailableXp += request.XpAmount;
                userStats.CumulativeXp += request.XpAmount;
                await _repository.SaveChangesAsync();

                _logger.LogInformation("User stats updated for explorer {ExplorerId}", request.ExplorerId);


                //Update Streak
                var streakResult = await _mediator.Send(new UpdateStreakCommand(request.ExplorerId, userStats), cancellationToken);
                if (!streakResult.IsSuccess)
                {
                    _logger.LogError("Failed to update streak for explorer {ExplorerId}. Error: {ErrorCode}", request.ExplorerId, streakResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<string>.Failure(streakResult.errorCode);
                }

                _logger.LogInformation("Streak updated for explorer {ExplorerId}", request.ExplorerId);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ApiResponse<string>.Success("Check-in stats updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating check-in stats for explorer {ExplorerId}", request.ExplorerId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
        }
    }
}
