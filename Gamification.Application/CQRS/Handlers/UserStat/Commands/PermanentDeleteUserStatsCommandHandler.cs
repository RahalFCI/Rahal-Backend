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
    public class PermanentDeleteUserStatsCommandHandler : IRequestHandler<PermanentDeleteUserStatsCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.UserStats> _repository;
        private readonly ILogger<PermanentDeleteUserStatsCommandHandler> _logger;

        public PermanentDeleteUserStatsCommandHandler(
            IGenericRepository<Domain.Entities.UserStats> repository,
            ILogger<PermanentDeleteUserStatsCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermanentDeleteUserStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Permanently deleting user stats for explorer {ExplorerId}", request.UserId);

            var userStats = await _repository.GetTable()
                .FirstOrDefaultAsync(us => us.ExplorerProfileId == request.UserId, cancellationToken);
            
            if (userStats is null)
            {
                _logger.LogWarning("User stats for explorer {ExplorerId} not found", request.UserId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(userStats);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User stats for explorer {ExplorerId} permanently deleted", request.UserId);

            return ApiResponse<string>.Success("User stats permanently deleted successfully");
        }
    }
}
