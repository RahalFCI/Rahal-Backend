using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.Achievements.Commands
{
    public class RestoreDeletedAchievementCommandHandler : IRequestHandler<RestoreDeletedAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<RestoreDeletedAchievementCommandHandler> _logger;

        public RestoreDeletedAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<RestoreDeletedAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted achievement {AchievementId}", request.Id);

            var achievementExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(a => a.Id == request.Id && a.IsDeleted, cancellationToken);

            if (!achievementExists)
            {
                _logger.LogWarning("Deleted achievement {AchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Achievement achievement = new Achievement()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(achievement, nameof(achievement.IsDeleted), nameof(achievement.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} restored successfully", request.Id);

            return ApiResponse<string>.Success("Achievement restored successfully");
        }
    }
}
