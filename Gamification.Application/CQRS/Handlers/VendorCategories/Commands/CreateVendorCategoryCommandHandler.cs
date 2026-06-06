using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Commands.VendorCategories;
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
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Commands
{
    public class CreateVendorCategoryCommandHandler : IRequestHandler<CreateVendorCategoryCommand, ApiResponse<GetVendorCategoryDto>>
    {
        private readonly IGamificationRepository<VendorCategory> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreateVendorCategoryCommandHandler> _logger;

        public CreateVendorCategoryCommandHandler(
            IGamificationRepository<VendorCategory> repository,
            ICacheService cacheService,
            ILogger<CreateVendorCategoryCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorCategoryDto>> Handle(CreateVendorCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating Vendor Category with name {VendorCategoryName}", request.CategoryName);

            var existingCategoryName = await _repository.GetTable().Where(c => c.CategoryName == request.CategoryName).AnyAsync(cancellationToken);
            if (existingCategoryName)
            {
                _logger.LogWarning("Vendor Category with name {CategoryName} already exists", request.CategoryName);
                return ApiResponse<GetVendorCategoryDto>.Failure(ErrorCode.AlreadyExists);
            }

            VendorCategory category = new VendorCategory() { CategoryName = request.CategoryName };
            _repository.Add(category);
            await _repository.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync("vendor-categories:all");

            _logger.LogInformation("Category {CategoryId} created successfully", category.Id);

            var dto = VendorCategoryMapper.ToGetDto(category);
            return ApiResponse<GetVendorCategoryDto>.Success(dto);
        }
    }
}
