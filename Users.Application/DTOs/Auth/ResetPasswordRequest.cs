using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Application.DTOs.Auth
{
    public record ResetPasswordRequest()
    {
        public required string Email { get; init; }

        public required string Token { get; init; }

        public required string NewPassword { get; init; }

        public required string ConfirmPassword { get; init; }
    }
}
