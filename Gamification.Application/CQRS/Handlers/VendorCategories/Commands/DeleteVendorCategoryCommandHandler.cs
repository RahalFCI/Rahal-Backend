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

namespace Gamification.Application.CQRS.Handlers.VendorCategories.Commands
{
    internal class DeleteVendorCategoryCommandHandler : IRequestHandler<DeleteVendorCategoryCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<VendorCategory> _repository;
        private readonly ILogger<DeleteVendorCategoryCommandHandler> _logger;

        public DeleteVendorCategoryCommandHandler(
            IGenericRepository<VendorCategory> repository,
            ILogger<DeleteVendorCategoryCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteVendorCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting Vendor Category with Id {VendorCategoryId}", request.CategoryId);

            var category = await _repository.GetTable().Where(c => c.Id == request.CategoryId).FirstOrDefaultAsync(cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Vendor Category with name {CategoryName} doesn't exist", request.CategoryId);
                return ApiResponse<string>.Failure(ErrorCode.AlreadyExists);
            }

            _repository.Delete(category);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} deleted successfully", category.Id);

            return ApiResponse<string>.Success("Category deleted successfully.");
        }
    }
}
