using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands;
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

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    internal class RestoreDeletedVendorProfileCommandHandler : IRequestHandler<RestoreDeletedVendorProfileCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<RestoreDeletedVendorProfileCommandHandler> _logger;

        public RestoreDeletedVendorProfileCommandHandler(IGenericRepository<VendorProfile> repository, ILogger<RestoreDeletedVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Restoring vendor profile for user {UserId}", request.VendorId);
                var existingVendor = await _repository.GetTable().Where(x => x.UserId == request.VendorId).AnyAsync(cancellationToken);
                if (!existingVendor)
                {
                    _logger.LogError("Vendor profile does not exist for user {UserId}", request.VendorId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                VendorProfile vendorProfile = new VendorProfile
                {
                    UserId = request.VendorId,
                    DeletedAt = null,
                    IsDeleted = false
                };

                _repository.SaveInclude(vendorProfile, nameof(vendorProfile.IsDeleted), nameof(vendorProfile.DeletedAt));
                await _repository.SaveChangesAsync();

                _logger.LogError("Restored vendor profile for user {UserId}", request.VendorId);

                return ApiResponse<string>.Success("Vendor profile restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while restoring vendor profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
