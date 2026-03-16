using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Auth
{
    public record AuthRequestDto(string Email, string Password)
    {
    }
}
