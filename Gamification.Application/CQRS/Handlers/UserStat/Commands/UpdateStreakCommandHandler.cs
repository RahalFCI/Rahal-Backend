using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.Interfaces;
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
    public class UpdateStreakCommandHandler : IRequestHandler<UpdateStreakCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<UserStats> _userStatsRepository;
        private readonly ILogger<UpdateStreakCommandHandler> _logger;

        public UpdateStreakCommandHandler(
            IGamificationRepository<UserStats> userStatsRepository,
            ILogger<UpdateStreakCommandHandler> logger)
        {
            _userStatsRepository = userStatsRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateStreakCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating streak for explorer {ExplorerId}", request.ExplorerId);
            
            var stats = request.UserStats ?? await _userStatsRepository.GetTable()
                .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (stats is null)
            {
                _logger.LogInformation("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            // Calculate new streak based on last activity date
            var now = DateTime.UtcNow;
            var newStreak = stats.LastActivityDate.HasValue &&
                            (now - stats.LastActivityDate.Value).TotalHours <= 24
                ? stats.CurrentStreak + 1
                : 1;

            if (stats.LastActivityDate.HasValue && stats.LastActivityDate.Value.Date == now.Date)
                return ApiResponse<string>.Success("Streak already updated today");

            // Update the streak and longest streak if necessary
            await _userStatsRepository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(us => us.CurrentStreak, newStreak)
                    .SetProperty(us => us.LastActivityDate, now)
                    .SetProperty(us => us.LongestStreak, us => newStreak > us.LongestStreak
                        ? newStreak
                        : us.LongestStreak),
                cancellationToken);

            _logger.LogInformation("Streak updated successfully for explorer {ExplorerId}", request.ExplorerId);

            return ApiResponse<string>.Success("Streak updated successfully");
        }
    }
}
