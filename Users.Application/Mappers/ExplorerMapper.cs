using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Mappers
{
    internal class ExplorerMapper : IUserMapper<ExplorerDto, ExplorerSummaryDto>
    {
        public ExplorerDto ToDto(User user)
        {
            if (user?.ExplorerProfile == null)
                throw new InvalidOperationException("Explorer profile not found for user " + user?.Id);

            return new ExplorerDto(
                BirthDate: user.ExplorerProfile.BirthDate,
                gender: user.ExplorerProfile.Gender,
                Bio: user.ExplorerProfile.Bio,
                CountryCode: user.ExplorerProfile.CountryCode,
                AvailableXp: user.ExplorerProfile.AvailableXp,
                CumlativeXp: user.ExplorerProfile.CumulativeXp,
                Level: user.ExplorerProfile.Level,
                IsPublic: user.ExplorerProfile.IsPublic,
                IsPremium: user.ExplorerProfile.IsPremium
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

        public ExplorerSummaryDto ToSummary(User user)
        {
            if (user?.ExplorerProfile == null)
                throw new InvalidOperationException("Explorer profile not found for user " + user?.Id);

            return new ExplorerSummaryDto(
                Bio: user.ExplorerProfile.Bio,
                CumlativeXp: user.ExplorerProfile.CumulativeXp,
                Level: user.ExplorerProfile.Level,
                IsPublic: user.ExplorerProfile.IsPublic,
                IsPremium: user.ExplorerProfile.IsPremium
            )
            {
                Id = user.Id,
                Name = user.DisplayName,
                ProfilePictureUrl = user.ProfilePictureURL,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Role = user.UserType
            };
        }
    }
}
