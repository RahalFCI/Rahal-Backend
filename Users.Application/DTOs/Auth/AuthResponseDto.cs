using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Auth
{
    public record AuthResponseDto(string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiration,
        DateTime RefreshTokenExpiration
        )
    {
        public AuthResponseDto() : this(string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue)
        {
        }
    }
}
