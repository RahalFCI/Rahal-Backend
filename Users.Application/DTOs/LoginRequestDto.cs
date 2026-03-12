using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs
{
    internal record LoginRequestDto(string Email, string Password)
    {
    }
}
