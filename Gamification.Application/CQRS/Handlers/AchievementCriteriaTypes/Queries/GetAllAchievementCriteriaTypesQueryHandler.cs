using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.DTOs.VendorCategory;
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
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllAchievementCriteriaTypesQueryHandler> _logger;

        public GetAllAchievementCriteriaTypesQueryHandler(IGenericRepository<AchievementCriteriaType> repository, ICacheService cacheService, ILogger<GetAllAchievementCriteriaTypesQueryHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>> Handle(GetAllAchievementCriteriaTypesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all achievement criteria types from cache");

            var cached = await _cacheService.GetAsync<IEnumerable<GetAchievementCriteriaTypeDto>>("achievement-criteria-types:all");
            if (cached is not null) return ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>.Success(cached);

            _logger.LogInformation("Failed to fetch all achievement criteria types from cache");
            _logger.LogInformation("Fetching all achievement criteria types from database");


            var criteriaTypes = await _repository.GetAllAsync(cancellationToken);

            await _cacheService.SetAsync("achievement-criteria-types:all", criteriaTypes, TimeSpan.FromHours(1));
            _logger.LogInformation("Cached all achievement criteria types");

            var criteriaTypeDtos = AchievementCriteriaTypeMapper.ToGetDtos(criteriaTypes);
            return ApiResponse<IEnumerable<GetAchievementCriteriaTypeDto>>.Success(criteriaTypeDtos);
        }
    }
}
