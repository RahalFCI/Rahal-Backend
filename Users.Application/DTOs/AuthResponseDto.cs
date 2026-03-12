using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs
{
    internal record AuthResponseDto(string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiration,
        DateTime RefreshTokenExpiration
        )
    {
    }
}
