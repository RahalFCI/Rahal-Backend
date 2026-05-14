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
    public class DeleteUserStatsCommandHandler : IRequestHandler<DeleteUserStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ILogger<DeleteUserStatsCommandHandler> _logger;

        public DeleteUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<DeleteUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting user stats for explorer {ExplorerId}", request.UserId);

            var userStatsExists = await _repository.GetTable()
                .AnyAsync(us => us.ExplorerProfileId == request.UserId, cancellationToken);
            
            if (!userStatsExists)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.UserId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var userStats = new Domain.Entities.UserStats
            {
                ExplorerProfileId = request.UserId,
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _repository.SaveInclude(userStats, nameof(userStats.IsDeleted), nameof(userStats.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats for explorer {ExplorerId} deleted successfully", request.UserId);

            return ApiResponse<string>.Success("User stats deleted successfully");
        }
    }
}
