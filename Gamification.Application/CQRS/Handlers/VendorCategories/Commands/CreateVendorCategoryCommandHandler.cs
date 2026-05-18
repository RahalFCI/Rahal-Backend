using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Commands.VendorCategories;
using Gamification.Application.CQRS.Queries.AchievementCriteriaType;
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

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Commands
{
    public class CreateVendorCategoryCommandHandler : IRequestHandler<CreateVendorCategoryCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<VendorCategory> _repository;
        private readonly ILogger<CreateVendorCategoryCommandHandler> _logger;

        public CreateVendorCategoryCommandHandler(
            IGenericRepository<VendorCategory> repository,
            ILogger<CreateVendorCategoryCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CreateVendorCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating Vendor Category with name {VendorCategoryName}", request.CategoryName);

            var existingCategoryName = await _repository.GetTable().Where(c => c.CategoryName == request.CategoryName).AnyAsync(cancellationToken);
            if (existingCategoryName)
            {
                _logger.LogWarning("Vendor Category with name {CategoryName} already exists", request.CategoryName);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            VendorCategory category = new VendorCategory() { CategoryName = request.CategoryName };
            _repository.Add(category);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} created successfully", category.Id);

            return ApiResponse<string>.Success($"Category created successfully. ID: {category.Id}");
        }
    }
}
