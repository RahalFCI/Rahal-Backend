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

namespace Gamification.Application.CQRS.Handlers.AchievementCriteriaTypes.Queries
{
    public class GetAchievementCriteriaTypeByIdQueryHandler : IRequestHandler<GetAchievementCriteriaTypeByIdQuery, ApiResponse<GetAchievementCriteriaTypeDto?>>
    {
        private readonly IGenericRepository<AchievementCriteriaType> _repository;
        private readonly ILogger<GetAchievementCriteriaTypeByIdQueryHandler> _logger;

        public GetAchievementCriteriaTypeByIdQueryHandler(IGenericRepository<AchievementCriteriaType> repository, ILogger<GetAchievementCriteriaTypeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetAchievementCriteriaTypeDto?>> Handle(GetAchievementCriteriaTypeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching achievement criteria type {CriteriaId}", request.Id);

            var criteriaType = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (criteriaType is null)
            {
                _logger.LogWarning("Criteria type {CriteriaTypeId} not found", request.Id);
                return ApiResponse<GetAchievementCriteriaTypeDto?>.Failure(ErrorCode.NotFound);
            }

            var criteriaTypeDto = AchievementCriteriaTypeMapper.ToGetDto(criteriaType);
            return ApiResponse<GetAchievementCriteriaTypeDto?>.Success(criteriaTypeDto);
        }
    }
}
