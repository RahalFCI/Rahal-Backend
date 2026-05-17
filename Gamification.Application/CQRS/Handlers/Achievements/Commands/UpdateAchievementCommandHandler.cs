using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.Mappers;
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
    public class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAchievementCommandHandler> _logger;

        public UpdateAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            IMediator mediator,
            ILogger<UpdateAchievementCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating achievement {AchievementId}", request.Id);

            var achievement = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var badge = await _mediator.Send(new GetBadgeByIdQuery(request.Dto.BadgeId), cancellationToken);
            if (!badge.IsSuccess)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Dto.BadgeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var criteriaType = await _mediator.Send(new GetAchievementCriteriaTypeByIdQuery(request.Dto.CriteriaTypeId), cancellationToken);
            if (criteriaType is null)
            {
                _logger.LogWarning("Criteria type {CriteriaTypeId} not found", request.Dto.CriteriaTypeId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var existingAchievement = await _repository.GetTable().Where(a => a.Title == request.Dto.Title).AnyAsync(cancellationToken);
            if (existingAchievement)
            {
                _logger.LogWarning("Achievement with title {AchievementTitle} already exists", request.Dto.Title);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            AchievementMapper.UpdateEntity(achievement, request.Dto);
            _repository.Update(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} updated successfully", request.Id);

            return ApiResponse<string>.Success("Achievement updated successfully");
        }
    }
}
