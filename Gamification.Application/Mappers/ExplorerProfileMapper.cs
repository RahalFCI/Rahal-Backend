using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.ExplorerAchievement;
using Gamification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.Mappers
{
    public static class ExplorerProfileMapper
    {
        public static GetExplorerDto ToGetDto(ExplorerProfile explorerProfile)
        {
            return new GetExplorerDto
            {
                UserId = explorerProfile.UserId,
                AvailableXp = explorerProfile.AvailableXp,
                Bio = explorerProfile.Bio,
                BirthDate = explorerProfile.BirthDate,
                CountryCode = explorerProfile.CountryCode,
                CumlativeXp = explorerProfile.CumulativeXp,
                gender = explorerProfile.Gender,
                IsPremium = explorerProfile.IsPremium,
                IsPublic = explorerProfile.IsPublic,
                Level = explorerProfile.Level,
            };
        }

        public static ExplorerProfile ToEntity(AddExplorerDto dto)
        {
            return new ExplorerProfile
            {
                UserId = dto.UserId,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
                Bio = dto.Bio,
                CountryCode = dto.CountryCode,
                IsPublic = dto.IsPublic,
                IsPremium = dto.IsPremium,

            };
        }

        public static ExplorerProfile ToEntity(UpdateExplorerDto dto)
        {
            return new ExplorerProfile
            {
                UserId = dto.UserId,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
                Bio = dto.Bio,
                CountryCode = dto.CountryCode,
                IsPublic = dto.IsPublic,
                IsPremium = dto.IsPremium,
                CumulativeXp = dto.CumlativeXp,
                AvailableXp = dto.AvailableXp,
                Level = dto.Level,

            };
        }

        public static IEnumerable<GetExplorerDto> ToGetDtos(IEnumerable<ExplorerProfile?> explorerProfiles)
        {
            return explorerProfiles.Where(e => e is not null).Select(e => ToGetDto(e!));
        }
    }
}
