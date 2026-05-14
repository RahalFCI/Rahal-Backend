using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Register
{
    public record BaseRegisterDto(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        UserRoleEnum UserRole)
    {
        public BaseRegisterDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, default)
        {
        }
    }
}
