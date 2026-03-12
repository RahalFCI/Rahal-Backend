using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs
{
    public record UserSummaryDto(Guid Id,
        string Name,
        string Bio,
        int CumlativeXp,
        int Level,
        bool IsPublic,
        bool IsPremium,
        string ProfilePictureUrl
        //TODO: Add Plan Tier
        )
    {
    }
}
