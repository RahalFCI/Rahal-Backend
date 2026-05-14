using Gamification.Application.CQRS.Commands.UserStats;
using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public class UpdateUserStatsCommandHandler : IRequestHandler<UpdateUserStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ILogger<UpdateUserStatsCommandHandler> _logger;

        public UpdateUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<UpdateUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating user stats for explorer {ExplorerId}", request.UserId);

            var userStats = await _repository.GetTable()
                .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.UserId, cancellationToken);

            if (userStats is null)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.UserId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            userStats.TotalCheckInCount = request.Dto.TotalCheckIns;
            userStats.TotalChallengeCount = request.Dto.TotalChallengesCompleted;
            userStats.TotalAchievementCount = request.Dto.TotalAchievementsEarned;
            userStats.TotalBadgeCount = request.Dto.TotalBadgesEarned;
            userStats.LongestStreak = request.Dto.LongestStreak;
            userStats.LastActivityDate = request.Dto.LastActivityDate ?? DateTime.UtcNow;

            _repository.Update(userStats);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats for explorer {ExplorerId} updated successfully", request.UserId);

            return ApiResponse<string>.Success("User stats updated successfully");
        }
    }
}
