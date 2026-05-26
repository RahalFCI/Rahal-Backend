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

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    public class CreateVendorProfileCommandHandler : IRequestHandler<CreateVendorProfileCommand, ApiResponse<GetVendorDto>>
    {
        private readonly IGenericRepository<VendorProfile> _repository;
        private readonly ILogger<CreateVendorProfileCommandHandler> _logger;

        public CreateVendorProfileCommandHandler(IGenericRepository<VendorProfile> repository, ILogger<CreateVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorDto>> Handle(CreateVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Creating vendor profile for user {UserId}", request.VendorProfileDto.UserId);
                var vendorProfile = VendorProfileMapper.ToEntity(request.VendorProfileDto);

                var existingVendor = await _repository.GetTable().Where(x => x.UserId == request.VendorProfileDto.UserId).AnyAsync(cancellationToken);
                if (!existingVendor)
                {
                    _logger.LogError("Vendor profile already exists for user {UserId}", request.VendorProfileDto.UserId);
                    return ApiResponse<GetVendorDto>.Failure(ErrorCode.AlreadyExists);
                }

                vendorProfile.ProfilePictureURL = request.ProfilePictureUrl;

                _repository.Add(vendorProfile);
                await _repository.SaveChangesAsync();

                _logger.LogError("Created vendor profile for user {UserId}", request.VendorProfileDto.UserId);

                var dto = VendorProfileMapper.ToGetDto(vendorProfile);
                return ApiResponse<GetVendorDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating vendor profile");
                return ApiResponse<GetVendorDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
