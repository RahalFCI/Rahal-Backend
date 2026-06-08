using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.Explorer
{
    public record UpdateExplorerDto(
        Guid UserId,
        string DisplayName,
        DateOnly BirthDate,
        GenderEnum Gender,
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
        public UpdateExplorerDto() : this(default, string.Empty, default, default, string.Empty, string.Empty, default, default, default, default, default)
        {
        }
    }
}
