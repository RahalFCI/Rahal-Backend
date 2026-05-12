using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.Achievements.Commands
{
    public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<DeleteAchievementCommandHandler> _logger;

        public DeleteAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            ILogger<DeleteAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting achievement {AchievementId}", request.Id);

            var achievementExists = await _repository.GetTable().AnyAsync(a => a.Id == request.Id, cancellationToken);
            if (!achievementExists)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Achievement achievement = new Achievement()
            {
                Id = request.Id,
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _repository.SaveInclude(achievement, nameof(achievement.IsDeleted), nameof(achievement.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Achievement deleted successfully");
        }
    }
}
