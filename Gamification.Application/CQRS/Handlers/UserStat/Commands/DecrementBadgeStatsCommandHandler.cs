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
    public class DecrementBadgeStatsCommandHandler : IRequestHandler<DecrementBadgeStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly ILogger<DecrementBadgeStatsCommandHandler> _logger;

        public DecrementBadgeStatsCommandHandler(
            IGenericRepository<UserStats> repository,
            ILogger<DecrementBadgeStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DecrementBadgeStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Decrementing badge count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId && us.TotalBadgeCount > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalBadgeCount, us => us.TotalBadgeCount - 1), cancellationToken);

            if(rowsAffected == 0)
            {
                _logger.LogError("Failed to decrement badge count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Badge count decremented for explorer {ExplorerId}", request.ExplorerId);

            return ApiResponse<string>.Success("Badge stats decremented successfully");
        }
    }
}
