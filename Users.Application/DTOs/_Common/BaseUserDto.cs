using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs._Common
{
    public record BaseUserDto(Guid Id,
        string Name,
        string Email,
        string PhoneNumber,
        string ProfilePictureUrl,
        UserRoleEnum Role)
    {
        public BaseUserDto() : this(default, string.Empty, string.Empty, string.Empty, string.Empty, default) { }

    }
}
