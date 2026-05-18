using Gamification.Application.CQRS.Commands.Achievement;
using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Handlers.Achievements.Commands;
using Gamification.Application.CQRS.Queries.AchievementCriteriaType;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.AchievementCriteriaTypes.Commands
{
    public class CreateAchievementCriteriaTypeCommandHandler : IRequestHandler<CreateAchievementCriteriaTypeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<AchievementCriteriaType> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateAchievementCriteriaTypeCommandHandler> _logger;

        public CreateAchievementCriteriaTypeCommandHandler(
            IGenericRepository<AchievementCriteriaType> repository,
            IMediator mediator,
            ILogger<CreateAchievementCriteriaTypeCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateAchievementCriteriaTypeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating achievement criteria type {AchievementCriteriaTypeTitle}", request.Dto.Name);

            var existingCriteriaType = await _mediator.Send(new GetAchievementCriteriaTypeByNameQuery(request.Dto.Name), cancellationToken);
            if (existingCriteriaType.IsSuccess)
            {
                _logger.LogWarning("Achievement criteria type {AchievementCriteriaTypeTitle} already exists", request.Dto.Name);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            var achievement = AchievementCriteriaTypeMapper.ToEntity(request.Dto);
            _repository.Add(achievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Achievement {AchievementId} created successfully", achievement.Id);

            return ApiResponse<string>.Success($"Achievement created successfully. ID: {achievement.Id}");
        }
    }
}
