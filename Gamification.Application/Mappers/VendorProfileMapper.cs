using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.ExplorerAchievement;
using Gamification.Application.DTOs.Vendor;
using Gamification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Mappers
{
    public static class VendorProfileMapper
    {
        public static GetVendorDto ToGetDto(VendorProfile vendorProfile)
        {
            return new GetVendorDto
            {
                UserId = vendorProfile.UserId,
                DisplayName = vendorProfile.DisplayName,
                ProfilePictureUrl = vendorProfile.ProfilePictureURL,
                Address = vendorProfile.Address,
                AddressUrl = vendorProfile.AddressUrl,
                WorkingHours = vendorProfile.WorkingHours,
                CountryCode = vendorProfile.CountryCode,
                CategoryId = vendorProfile.CategoryId,
                IsApproved = vendorProfile.IsApproved
            };
        }

        public static VendorProfile ToEntity(AddVendorDto dto)
        {
            return new VendorProfile
            {
                UserId = dto.UserId,
                DisplayName = dto.DisplayName,
                Address = dto.Address,
                AddressUrl = dto.AddressUrl,
                WorkingHours = dto.WorkingHours,
                CountryCode = dto.CountryCode,
                CategoryId = dto.CategoryId,
            };
        }

        public static VendorProfile ToEntity(UpdateVendorDto dto)
        {
            return new VendorProfile
            {
                UserId = dto.UserId,
                DisplayName = dto.DisplayName,
                Address = dto.Address,
                AddressUrl = dto.AddressUrl,
                WorkingHours = dto.WorkingHours,
                CountryCode = dto.CountryCode,
                CategoryId = dto.CategoryId
            };
        }

        public static IEnumerable<GetVendorDto> ToGetDtos(IEnumerable<VendorProfile?> vendorProfiles)
        {
            return vendorProfiles.Where(e => e is not null).Select(e => ToGetDto(e!));
        }
    }
}
