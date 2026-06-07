using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.DTOs.Explorer
{
    public record AddExplorerDto(
            string DisplayName,
            Guid UserId,
            DateOnly BirthDate,
            GenderEnum Gender,
            string Bio,
            string CountryCode,
            bool IsPublic,
            bool IsPremium
            //TODO: Add Plan Tier
            )
    {
        public AddExplorerDto() : this(string.Empty, default, default, default, string.Empty, string.Empty, default, default)
        {
        }
    }
}
