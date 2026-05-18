using Gamification.Application.CQRS.Queries.VendorCategories;
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

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Queries
{
    internal class GetVendorCategoryByIdQueryHandler : IRequestHandler<GetVendorCategoryByIdQuery, ApiResponse<GetVendorCategoryDto>>
    {
        private readonly IGenericRepository<VendorCategory> _repository;
        private readonly ILogger<GetVendorCategoryByIdQueryHandler> _logger;

        public GetVendorCategoryByIdQueryHandler(IGenericRepository<VendorCategory> repository, ILogger<GetVendorCategoryByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<ApiResponse<GetVendorCategoryDto>> Handle(GetVendorCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching vendor category by ID: {Id}", request.Id);

            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (category is null)
            {
                return ApiResponse<GetVendorCategoryDto>.Failure(ErrorCode.NotFound);
            }

            var categoryDto = VendorCategoryMapper.ToGetDto(category);
            return ApiResponse<GetVendorCategoryDto>.Success(categoryDto);
        }
    }
}
