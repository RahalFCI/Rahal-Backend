using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Register
{
    public record RegisterAdminDto(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber,
        string ProfilePictureUrl)
    {
        public RegisterAdminDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }
    }
}
