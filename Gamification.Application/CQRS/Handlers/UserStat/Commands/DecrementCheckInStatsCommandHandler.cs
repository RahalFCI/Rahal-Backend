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
    public class DecrementCheckInStatsCommandHandler : IRequestHandler<DecrementCheckInStatsCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<UserStats> _repository;
        private readonly ILogger<DecrementCheckInStatsCommandHandler> _logger;

        public DecrementCheckInStatsCommandHandler(
            IGamificationRepository<UserStats> repository,
            ILogger<DecrementCheckInStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DecrementCheckInStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Decrementing check-in count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId && us.TotalCheckInCount > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalCheckInCount, us => us.TotalCheckInCount - 1), cancellationToken);

            if(rowsAffected == 0)
            {
                _logger.LogError("Failed to decrement check-in count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Check-in count decremented for explorer {ExplorerId}", request.ExplorerId);

            return ApiResponse<string>.Success("Check-in stats decremented successfully");
        }
    }
}
