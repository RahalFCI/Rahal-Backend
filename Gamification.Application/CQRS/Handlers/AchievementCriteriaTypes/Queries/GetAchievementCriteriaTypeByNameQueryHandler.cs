using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.Mappers;
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
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.AchievementCriteriaTypes.Queries
{
    public class GetAchievementCriteriaTypeByNameQueryHandler : IRequestHandler<GetAchievementCriteriaTypeByNameQuery, ApiResponse<GetAchievementCriteriaTypeDto?>>
    {
        private readonly IGamificationRepository<AchievementCriteriaType> _repository;
        private readonly ILogger<GetAchievementCriteriaTypeByNameQueryHandler> _logger;

        public GetAchievementCriteriaTypeByNameQueryHandler(IGamificationRepository<AchievementCriteriaType> repository, ILogger<GetAchievementCriteriaTypeByNameQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetAchievementCriteriaTypeDto?>> Handle(GetAchievementCriteriaTypeByNameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching achievement criteria type {CriteriaName}", request.Name);

            var criteriaType = await _repository.GetTable().Where(ct => ct.Name == request.Name).FirstOrDefaultAsync(cancellationToken);
            if (criteriaType is null)
            {
                _logger.LogWarning("Criteria type {CriteriaName} not found", request.Name);
                return ApiResponse<GetAchievementCriteriaTypeDto?>.Failure(ErrorCode.NotFound);
            }

            var criteriaTypeDto = AchievementCriteriaTypeMapper.ToGetDto(criteriaType);
            return ApiResponse<GetAchievementCriteriaTypeDto?>.Success(criteriaTypeDto);
        }
    }
}
