using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.Explorer
{
    public record GetExplorerDto(
        Guid UserId,
        DateOnly BirthDate,
        GenderEnum gender,
        string Bio,
        string CountryCode,
        int AvailableXp,
        int CumlativeXp,
        int Level,
        bool IsPublic,
        bool IsPremium
        //TODO: Add Plan Tier
        )
    {
        public GetExplorerDto() : this(default, default, default, string.Empty, string.Empty, default, default, default, default, default)
        {
        }
    }
}
