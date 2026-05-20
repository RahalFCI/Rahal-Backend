using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.UserStat.Commands
{
    public class RestoreDeletedUserStatsCommandHandler : IRequestHandler<RestoreDeletedUserStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RestoreDeletedUserStatsCommandHandler> _logger;

        public RestoreDeletedUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ICacheService cacheService,
            ILogger<RestoreDeletedUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted user stats for explorer {ExplorerId}", request.UserId);

            var userStats = await _repository.GetTable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.UserId && us.IsDeleted, cancellationToken);
            
            if (userStats is null)
            {
                _logger.LogWarning("Deleted user stats for explorer {ExplorerId} not found", request.UserId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            userStats.IsDeleted = false;
            userStats.DeletedAt = null;
            
            await _repository.SaveChangesAsync(cancellationToken);

            // Update cache
            await _cacheService.SortedSetAddAsync("leaderboard:xp", request.UserId.ToString(), userStats.CumulativeXp);

            _logger.LogInformation("User stats for explorer {ExplorerId} restored successfully", request.UserId);

            return ApiResponse<string>.Success("User stats restored successfully");
        }
    }
}
