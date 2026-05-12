using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands;
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
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    public class UpdateVendorProfileCommandHandler : IRequestHandler<UpdateVendorProfileCommand, ApiResponse<GetVendorDto>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<UpdateVendorProfileCommandHandler> _logger;

        public UpdateVendorProfileCommandHandler(IGenericRepository<VendorProfile> repository, ILogger<UpdateVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorDto>> Handle(UpdateVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Updating vendor profile for user {UserId}", request.UpdateVendorDto.UserId);
                var existingVendor = await _repository.GetTable().Where(x => x.UserId == request.UpdateVendorDto.UserId).FirstOrDefaultAsync(cancellationToken);
                if (existingVendor is not null)
                {
                    _logger.LogError("Vendor profile already exists for user {UserId}", request.UpdateVendorDto.UserId);
                    return ApiResponse<GetVendorDto>.Failure(ErrorCode.AlreadyExists);
                }

                var vendorProfile = VendorProfileMapper.ToEntity(request.UpdateVendorDto);
                _repository.Update(vendorProfile);
                await _repository.SaveChangesAsync();

                _logger.LogError("Updated vendor profile for user {UserId}", request.UpdateVendorDto.UserId);

                return ApiResponse<GetVendorDto>.Success(VendorProfileMapper.ToGetDto(vendorProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating vendor profile");
                return ApiResponse<GetVendorDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
