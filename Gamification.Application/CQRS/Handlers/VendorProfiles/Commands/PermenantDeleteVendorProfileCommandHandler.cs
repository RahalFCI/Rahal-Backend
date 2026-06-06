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
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    public class PermenantDeleteVendorProfileCommandHandler : IRequestHandler<PermenantDeleteVendorProfileCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<VendorProfile> _repository;
        private readonly ILogger<PermenantDeleteVendorProfileCommandHandler> _logger;

        public PermenantDeleteVendorProfileCommandHandler(IGamificationRepository<VendorProfile> repository, ILogger<PermenantDeleteVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermenantDeleteVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Deleting vendor profile for user {UserId}", request.Id);
                var vendorProfile = await _repository.GetTable().Where(x => x.UserId == request.Id).FirstOrDefaultAsync(cancellationToken);
                if (vendorProfile is null)
                {
                    _logger.LogError("Vendor profile does not exist for user {UserId}", request.Id);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                _repository.Delete(vendorProfile);
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
