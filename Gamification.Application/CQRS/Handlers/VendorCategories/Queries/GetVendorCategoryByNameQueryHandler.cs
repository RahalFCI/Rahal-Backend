using Gamification.Application.CQRS.Queries.VendorCategories;
using Gamification.Application.DTOs.VendorCategory;
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

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Queries
{
    internal class GetVendorCategoryByNameQueryHandler : IRequestHandler<GetVendorCategoryByNameQuery, ApiResponse<GetVendorCategoryDto>>
    {
        private readonly IGenericRepository<VendorCategory> _repository;
        private readonly ILogger<GetVendorCategoryByNameQueryHandler> _logger;

        public GetVendorCategoryByNameQueryHandler(IGenericRepository<VendorCategory> repository, ILogger<GetVendorCategoryByNameQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetVendorCategoryDto>> Handle(GetVendorCategoryByNameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching vendor category by name: {Name}", request.Name);

            var category = await _repository.GetTable().Where(c => c.CategoryName == request.Name).FirstOrDefaultAsync(cancellationToken);
            if (category is null)
            {
                return ApiResponse<GetVendorCategoryDto>.Failure(ErrorCode.NotFound);
            }

            var categoryDto = VendorCategoryMapper.ToGetDto(category);
            return ApiResponse<GetVendorCategoryDto>.Success(categoryDto);
        }
    }
}
