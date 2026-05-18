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

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Queries
{
    internal class GetAllVendorCategoriesQueryHandler : IRequestHandler<GetAllVendorCategoriesQuery, ApiResponse<IEnumerable<GetVendorCategoryDto>>>
    {
        private readonly IGenericRepository<VendorCategory> _repository;
        private readonly ILogger<GetAllVendorCategoriesQueryHandler> _logger;

        public GetAllVendorCategoriesQueryHandler(IGenericRepository<VendorCategory> repository, ILogger<GetAllVendorCategoriesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<GetVendorCategoryDto>>> Handle(GetAllVendorCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all vendor categories");

            var categories = await _repository.GetAllAsync(cancellationToken);

            var categoryDtos = VendorCategoryMapper.ToGetDtos(categories);
            return ApiResponse<IEnumerable<GetVendorCategoryDto>>.Success(categoryDtos);
        }
    }
}
