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
    public class DeleteVendorProfileCommandHandler : IRequestHandler<DeleteVendorProfileCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<DeleteVendorProfileCommandHandler> _logger;

        public DeleteVendorProfileCommandHandler(IGenericRepository<VendorProfile> repository, ILogger<DeleteVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Deleting vendor profile for user {UserId}", request.Id);
                var existingVendor = await _repository.GetTable().Where(x => x.UserId == request.Id).AnyAsync(cancellationToken);
                if (!existingVendor)
                {
                    _logger.LogError("Vendor profile does not exist for user {UserId}", request.Id);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                VendorProfile vendorProfile = new VendorProfile
                {
                    UserId = request.Id,
                    IsDeleted = true
                };

                _repository.SaveInclude(vendorProfile, nameof(vendorProfile.IsDeleted));
                await _repository.SaveChangesAsync();

                _logger.LogError("Deleted vendor profile for user {UserId}", request.Id);

                return ApiResponse<string>.Success("Vendor profile deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting vendor profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
