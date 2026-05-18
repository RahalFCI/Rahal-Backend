using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.UserStats;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Orchestrators.UserStat;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.UserStat.Orchestrators
{
    public class UpdateBadgeStatsOrchestratorHandler : IRequestHandler<UpdateBadgeStatsOrchestrator, ApiResponse<string>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateBadgeStatsOrchestratorHandler> _logger;

        public UpdateBadgeStatsOrchestratorHandler(
            IGenericRepository<UserStats> repository,
            IMediator mediator,
            ILogger<UpdateBadgeStatsOrchestratorHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateBadgeStatsOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Incrementing badge count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalBadgeCount, us => us.TotalBadgeCount + 1), cancellationToken);

            if(rowsAffected == 0)
            {
                _logger.LogError("Failed to increment badge count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Badge count incremented for explorer {ExplorerId}", request.ExplorerId);

            await _mediator.Send(new UpdateStreakCommand(request.ExplorerId), cancellationToken);

            return ApiResponse<string>.Success("Badge stats updated successfully");
        }
    }
}
