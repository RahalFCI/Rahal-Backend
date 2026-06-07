using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MassTransit;
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
    public class UpdateAchievementCriteriaTypeCommandHandler : IRequestHandler<UpdateAchievementCriteriaTypeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<AchievementCriteriaType> _repository;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateAchievementCriteriaTypeCommandHandler> _logger;

        public UpdateAchievementCriteriaTypeCommandHandler(
            IGamificationRepository<AchievementCriteriaType> repository,
            IMediator mediator,
            ICacheService cacheService,
            ILogger<UpdateAchievementCriteriaTypeCommandHandler> logger)
        {
            _repository = repository;
            _mediator = mediator;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateAchievementCriteriaTypeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating achievement criteria type {AchievementCriteriaTypeTitle}", request.Dto.Name);

            var achievementCriteriaType = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (achievementCriteriaType is null)
            {
                _logger.LogWarning("Achievement criteria type {AchievementCriteriaTypeTitle} not found", request.Dto.Name);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            achievementCriteriaType.Name = request.Dto.Name;
            achievementCriteriaType.Description = request.Dto.Description;
             
            await _repository.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync("achievement-criteria-types:all");

            _logger.LogInformation("Achievement criteria type {AchievementId} updated successfully", achievementCriteriaType.Id);

            return ApiResponse<string>.Success($"Achievement criteria type updated successfully. ID: {achievementCriteriaType.Id}");
        }
    }
}
