using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Admin;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    internal class AdminMapper : IUserMapper<AdminDto, AdminSummaryDto>
    {
        public AdminDto ToDto(User user)
        {
            if (user?.AdminProfile == null)
                throw new InvalidOperationException("Admin profile not found for user " + user?.Id);

            return new AdminDto()
            {
                Id = user.Id,
                Name = user.DisplayName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                ProfilePictureUrl = user.ProfilePictureURL,
                Role = user.UserType
            };
        }

        public AdminSummaryDto ToSummary(User user)
        {
            if (user?.AdminProfile == null)
                throw new InvalidOperationException("Admin profile not found for user " + user?.Id);

            return new AdminSummaryDto()
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
