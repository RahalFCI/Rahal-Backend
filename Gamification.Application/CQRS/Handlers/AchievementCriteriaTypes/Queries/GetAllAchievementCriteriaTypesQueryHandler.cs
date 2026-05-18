using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
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
    public class GetAllAchievementCriteriaTypesQueryHandler : IRequestHandler<GetAllAchievementCriteriaTypesQuery, ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>>
    {
        private readonly IGenericRepository<AchievementCriteriaType> _repository;
        private readonly ILogger<GetAllAchievementCriteriaTypesQueryHandler> _logger;

        public GetAllAchievementCriteriaTypesQueryHandler(IGenericRepository<AchievementCriteriaType> repository, ILogger<GetAllAchievementCriteriaTypesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>> Handle(GetAllAchievementCriteriaTypesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all achievement criteria type");

            var criteriaTypes = await _repository.GetAllAsync(cancellationToken);

            var criteriaTypeDtos = AchievementCriteriaTypeMapper.ToGetDtos(criteriaTypes);
            return ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>.Success(criteriaTypeDtos);
        }
    }
}
