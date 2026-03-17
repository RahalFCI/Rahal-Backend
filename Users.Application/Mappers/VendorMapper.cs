using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Vendor;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    internal class VendorMapper : IUserMapper<VendorDto, VendorSummaryDto>
    {
        public VendorDto ToDto(User user)
        {
            if (user?.VendorProfile == null)
                throw new InvalidOperationException("Vendor profile not found for user " + user?.Id);

            return new VendorDto(
                CountryCode: user.VendorProfile.CountryCode,
                Address: user.VendorProfile.Address,
                AddressUrl: user.VendorProfile.AddressUrl,
                WorkingHours: user.VendorProfile.WorkingHours,
                CategoryId: user.VendorProfile.CategoryId,
                IsApproved: user.VendorProfile.IsApproved
            )
            {
                Id = user.Id,
                Name = user.DisplayName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                ProfilePictureUrl = user.ProfilePictureURL,
                Role = user.UserType
            };
        }

        public VendorSummaryDto ToSummary(User user)
        {
            if (user?.VendorProfile == null)
                throw new InvalidOperationException("Vendor profile not found for user " + user?.Id);

            return new VendorSummaryDto(
                CountryCode: user.VendorProfile.CountryCode,
                Address: user.VendorProfile.Address,
                AddressUrl: user.VendorProfile.AddressUrl,
                CategoryId: user.VendorProfile.CategoryId,
                IsApproved: user.VendorProfile.IsApproved
            )
            {
                Id = user.Id,
                Name = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureURL,
                PhoneNumber = user.PhoneNumber!,
                Email = user.Email!,
                Role = user.UserType
            };
        }
    }
}
