using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Explorer
{
    public record ExplorerSummaryDto(
        string Bio,
        int CumlativeXp,
        int Level,
        bool IsPublic,
        bool IsPremium
        //TODO: Add Plan Tier
        ) : BaseUserSummaryDto
    {
        public ExplorerSummaryDto() : this(string.Empty, default, default, false, false)
        {
        }
    }
}
