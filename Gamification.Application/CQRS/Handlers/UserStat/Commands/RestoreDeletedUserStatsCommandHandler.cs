using Gamification.Application.CQRS.Commands.UserStats;
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
        private readonly ILogger<RestoreDeletedUserStatsCommandHandler> _logger;

        public RestoreDeletedUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<RestoreDeletedUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted user stats for explorer {ExplorerId}", request.UserId);

            var userStatsExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(us => us.ExplorerProfileId == request.UserId && us.IsDeleted, cancellationToken);
            
            if (!userStatsExists)
            {
                _logger.LogWarning("Deleted user stats for explorer {ExplorerId} not found", request.UserId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            UserStats userStats = new UserStats()
            {
                ExplorerProfileId = request.UserId,
                IsDeleted = false,
                DeletedAt = null
            };
            

            _repository.SaveInclude(userStats, nameof(userStats.IsDeleted), nameof(userStats.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats for explorer {ExplorerId} restored successfully", request.UserId);

            return ApiResponse<string>.Success("User stats restored successfully");
        }
    }
}
