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
    public class DecrementChallengeStatsCommandHandler : IRequestHandler<DecrementChallengeStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<UserStats> _repository;
        private readonly ILogger<DecrementChallengeStatsCommandHandler> _logger;

        public DecrementChallengeStatsCommandHandler(
            IGenericRepository<UserStats> repository,
            ILogger<DecrementChallengeStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DecrementChallengeStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Decrementing challenge count for explorer {ExplorerId}", request.ExplorerId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.ExplorerId, cancellationToken);

            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var rowsAffected = await _repository.GetTable()
                .Where(us => us.ExplorerProfileId == request.ExplorerId && us.TotalChallengeCount > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(us => us.TotalChallengeCount, us => us.TotalChallengeCount - 1), cancellationToken);

            if(rowsAffected == 0)
            {
                _logger.LogError("Failed to decrement challenge count for explorer {ExplorerId}", request.ExplorerId);
                return ApiResponse<string>.Failure(ErrorCode.DatabaseError);
            }

            _logger.LogInformation("Challenge count decremented for explorer {ExplorerId}", request.ExplorerId);

            return ApiResponse<string>.Success("Challenge stats decremented successfully");
        }
    }
}
