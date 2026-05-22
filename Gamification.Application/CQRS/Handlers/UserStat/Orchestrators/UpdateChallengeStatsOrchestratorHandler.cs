using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.UserStat;
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
    public class UpdateChallengeStatsOrchestratorHandler : IRequestHandler<UpdateChallengeStatsOrchestrator, ApiResponse<string>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateChallengeStatsOrchestratorHandler> _logger;

        public UpdateChallengeStatsOrchestratorHandler(
            IGenericRepository<UserStats> repository,
            IMediator mediator,
            ILogger<UpdateChallengeStatsOrchestratorHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateChallengeStatsOrchestrator request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Incrementing challenge count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalChallengeCount, us => us.TotalChallengeCount + 1)
                .SetProperty(us => us.AvailableXp, us => us.AvailableXp + request.Xp)
                .SetProperty(us => us.CumulativeXp, us => us.CumulativeXp + request.Xp), cancellationToken);

            if (rowsAffected == 0)
            {
                _logger.LogError("Failed to increment challenge count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Challenge count incremented for explorer {ExplorerId}", request.ExplorerId);

            await _mediator.Send(new UpdateStreakCommand(request.ExplorerId), cancellationToken);

            return ApiResponse<string>.Success("Challenge stats updated successfully");
        }
    }
}
