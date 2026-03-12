using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs
{
    public record AuthResponseDto(string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiration,
        DateTime RefreshTokenExpiration
        )
    {
    }
}
