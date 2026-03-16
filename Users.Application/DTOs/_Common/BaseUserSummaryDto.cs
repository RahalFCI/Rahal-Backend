using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs._Common
{
    public record BaseUserSummaryDto(Guid Id,
        string Name,
        string ProfilePictureUrl,
        string PhoneNumber,
        string Email,
        UserRoleEnum Role)
    {
        public BaseUserSummaryDto() : this(default, string.Empty, string.Empty, string.Empty, string.Empty, default) { }
    }
}
