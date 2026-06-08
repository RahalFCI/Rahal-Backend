using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System.Net;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Commands
{
    public class UpdateVendorProfileCommandHandler : IRequestHandler<UpdateVendorProfileCommand, ApiResponse<GetVendorDto>>
    {
        private readonly IGamificationRepository<VendorProfile> _repository;
        private readonly ILogger<UpdateVendorProfileCommandHandler> _logger;

        public UpdateVendorProfileCommandHandler(IGamificationRepository<VendorProfile> repository, ILogger<UpdateVendorProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorDto>> Handle(UpdateVendorProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Updating vendor profile for user {UserId}", request.UpdateVendorDto.UserId);
                var vendorProfile = await _repository.GetTable().Where(x => x.UserId == request.UpdateVendorDto.UserId).FirstOrDefaultAsync(cancellationToken);
                if (vendorProfile is null)
                {
                    _logger.LogError("Vendor profile already exists for user {UserId}", request.UpdateVendorDto.UserId);
                    return ApiResponse<GetVendorDto>.Failure(ErrorCode.NotFound);
                }

                vendorProfile.DisplayName = request.UpdateVendorDto.DisplayName;
                vendorProfile.Address = request.UpdateVendorDto.Address;
                vendorProfile.AddressUrl = request.UpdateVendorDto.AddressUrl;
                vendorProfile.WorkingHours = request.UpdateVendorDto.WorkingHours;
                vendorProfile.CountryCode = request.UpdateVendorDto.CountryCode;
                vendorProfile.CategoryId = request.UpdateVendorDto.CategoryId;


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
