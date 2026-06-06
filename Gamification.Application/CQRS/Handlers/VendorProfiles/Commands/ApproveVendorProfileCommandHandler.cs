using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
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

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    public record ApproveVendorProfileCommandHandler : IRequestHandler<ApproveVendorProfileCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<VendorProfile> _repository;
        private readonly ILogger<ApproveVendorProfileCommandHandler> _logger;

        public ApproveVendorProfileCommandHandler(IGamificationRepository<VendorProfile> repository, ILogger<ApproveVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(ApproveVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Approving vendor profile for user {UserId}", request.VendorProfileId);
                var existingVendor = await _repository.GetTable().Where(x => x.Id == request.VendorProfileId).AnyAsync(cancellationToken);
                if (!existingVendor)
                {
                    _logger.LogError("Vendor profile does not exist for user {UserId}", request.VendorProfileId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                VendorProfile vendorProfile = new VendorProfile
                {
                    UserId = request.VendorProfileId,
                    IsApproved = true
                };

                _repository.SaveInclude(vendorProfile, nameof(vendorProfile.IsApproved));
                await _repository.SaveChangesAsync();

                _logger.LogError("Approved vendor profile for user {UserId}", request.VendorProfileId);

                return ApiResponse<string>.Success("Vendor approved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving vendor profile for user {UserId}", request.VendorProfileId);
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
