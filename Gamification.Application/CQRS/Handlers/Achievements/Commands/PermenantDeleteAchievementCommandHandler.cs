using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Achievements.Commands
{
    public class PermenantDeleteAchievementCommandHandler : IRequestHandler<PermenantDeleteAchievementCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<DeleteAchievementCommandHandler> _logger;

        public PermenantDeleteAchievementCommandHandler(
            IGamificationRepository<Domain.Entities.Achievement> repository,
            ILogger<DeleteAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting achievement {AchievementId}", request.Id);

            var achievement = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }


            _repository.Delete(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Achievement deleted successfully");
        }
    }
}
