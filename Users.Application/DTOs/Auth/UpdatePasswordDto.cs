using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Auth
{
    public record UpdatePasswordDto(
        string OldPassword,
        string NewPassword,
        string ConfirmPassword)
    {
        public UpdatePasswordDto() : this(string.Empty, string.Empty, string.Empty)
        {
        }
    }
}
