using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Achievement;
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
    public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, ApiResponse<GetAchievementDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Achievement> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateAchievementCommandHandler> _logger;

        public CreateAchievementCommandHandler(
            IGenericRepository<Domain.Entities.Achievement> repository,
            IMediator mediator,
            ILogger<CreateAchievementCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<GetAchievementDto>> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating achievement {AchievementTitle}", request.Dto.Title);

            var badge = await _mediator.Send(new GetBadgeByIdQuery(request.Dto.BadgeId), cancellationToken);
            if (!badge.IsSuccess)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Dto.BadgeId);
                return ApiResponse<GetAchievementDto>.Failure(ErrorCode.NotFound);
            }

            var criteriaType = await _mediator.Send(new GetAchievementCriteriaTypeByIdQuery(request.Dto.CriteriaTypeId), cancellationToken);
            if (!criteriaType.IsSuccess)
            {
                _logger.LogWarning("Criteria type {CriteriaTypeId} not found", request.Dto.CriteriaTypeId);
                return ApiResponse<GetAchievementDto>.Failure(ErrorCode.NotFound);
            }

            var existingAchievement = await _repository.GetTable().Where(a => a.Title == request.Dto.Title).AnyAsync(cancellationToken);
            if(existingAchievement)
            {
                _logger.LogWarning("Achievement with title {AchievementTitle} already exists", request.Dto.Title);
                return ApiResponse<GetAchievementDto>.Failure(ErrorCode.AlreadyExists);
            }

            var achievement = AchievementMapper.ToEntity(request.Dto);
            _repository.Add(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} created successfully", achievement.Id);

            var dto = AchievementMapper.ToGetDto(achievement);
            return ApiResponse<GetAchievementDto>.Success(dto);
        }
    }
}
