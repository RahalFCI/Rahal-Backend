using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs
{
    public record LoginRequestDto(string Email, string Password)
    {
    }
}
