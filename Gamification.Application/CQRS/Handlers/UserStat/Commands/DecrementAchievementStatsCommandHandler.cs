using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public class DecrementAchievementStatsCommandHandler : IRequestHandler<DecrementAchievementStatsCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<UserStats> _repository;
        private readonly ILogger<DecrementAchievementStatsCommandHandler> _logger;

        public DecrementAchievementStatsCommandHandler(
            IGamificationRepository<UserStats> repository,
            ILogger<DecrementAchievementStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DecrementAchievementStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Decrementing achievement count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId && us.TotalAchievementCount > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalAchievementCount, us => us.TotalAchievementCount - 1), cancellationToken);

            if(rowsAffected == 0)
            {
                _logger.LogError("Failed to decrement achievement count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Achievement count decremented for explorer {ExplorerId}", request.ExplorerId);

            return ApiResponse<string>.Success("Achievement stats decremented successfully");
        }
    }
}
