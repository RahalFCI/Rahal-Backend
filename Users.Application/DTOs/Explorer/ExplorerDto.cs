using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Enums;
namespace Users.Application.DTOs.Explorer
{
    public record ExplorerDto(
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
        ) : BaseUserDto
    {
        public ExplorerDto() : this(default, default, string.Empty, string.Empty, default, default, default, default, default)
        {
        }
    }
}
