using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;
{
    
}

namespace Users.Application.DTOs
{
    public record UserDto(Guid Id,
        string Name,
        string Email,
        string PhoneNumber,
        DateOnly BirthDate,
        GenderEnum gender,
        string Bio,
        string CountryCode,
        int AvailableXp,
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
