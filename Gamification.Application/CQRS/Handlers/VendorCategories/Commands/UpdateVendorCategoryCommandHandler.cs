using Gamification.Application.CQRS.Commands.VendorCategories;
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
    internal class UpdateVendorCategoryCommandHandler : IRequestHandler<UpdateVendorCategoryCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<VendorCategory> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateVendorCategoryCommandHandler> _logger;

        public UpdateVendorCategoryCommandHandler(
            IGamificationRepository<VendorCategory> repository,
            ICacheService cacheService,
            ILogger<UpdateVendorCategoryCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateVendorCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Vendor Category with Id {VendorCategoryId}", request.CategoryId);

            var existingCategoryName = await _repository.GetTable().Where(c => c.Id == request.CategoryId).AnyAsync(cancellationToken);
            if (!existingCategoryName)
            {
                _logger.LogWarning("Vendor Category with name {CategoryName} doesn't exist", request.CategoryName);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            VendorCategory category = new VendorCategory() {Id = request.CategoryId, CategoryName = request.CategoryName };
            _repository.SaveInclude(category, nameof(category.CategoryName));
            await _repository.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync("vendor-categories:all");

            _logger.LogInformation("Category {CategoryId} updated successfully", category.Id);

            return ApiResponse<string>.Success($"Category updated successfully. ID: {category.Id}");
        }
    }
}
