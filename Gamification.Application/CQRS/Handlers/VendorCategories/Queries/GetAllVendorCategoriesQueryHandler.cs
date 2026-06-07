using Gamification.Application.CQRS.Handlers.AchievementCriteriaTypes.Queries;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.VendorCategories;
using Gamification.Application.DTOs.AchievementCriteriaType;
using Gamification.Application.DTOs.VendorCategory;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Queries
{
    internal class GetAllVendorCategoriesQueryHandler : IRequestHandler<GetAllVendorCategoriesQuery, ApiResponse<IEnumerable<GetVendorCategoryDto>>>
    {
        private readonly IGamificationRepository<VendorCategory> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllVendorCategoriesQueryHandler> _logger;

        public GetAllVendorCategoriesQueryHandler(IGamificationRepository<VendorCategory> repository, ICacheService cacheService, ILogger<GetAllVendorCategoriesQueryHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<GetVendorCategoryDto>>> Handle(GetAllVendorCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all vendor categories from cache");

            var cached = await _cacheService.GetAsync<IEnumerable<GetVendorCategoryDto>>("vendor-categories:all");
            if (cached is not null) return ApiResponse<IEnumerable<GetVendorCategoryDto>>.Success(cached);

            _logger.LogInformation("Failed to fetch all vendor categories from cache");
            _logger.LogInformation("Fetching all vendor categories from database");

            var categories = await _repository.GetAllAsync(cancellationToken);

            await _cacheService.SetAsync("vendor-categories:all", categories, TimeSpan.FromHours(1));
            _logger.LogInformation("Cached all vendor categories");

            var categoryDtos = VendorCategoryMapper.ToGetDtos(categories);
            return ApiResponse<IEnumerable<GetVendorCategoryDto>>.Success(categoryDtos);
        }
    }
}
