using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Auth
{
    public record TokenDto(string AccessToken, string RefreshToken)
    {
        public TokenDto() : this(string.Empty, string.Empty)
        {
        }
    }
}
