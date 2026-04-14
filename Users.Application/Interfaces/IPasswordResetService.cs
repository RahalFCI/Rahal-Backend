using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Shared.Application.DTOs;
using Users.Application.DTOs.Auth;

namespace Users.Application.Interfaces
{
    public interface IPasswordResetService
    {
        Task ForgotPasswordAsync(string email, CancellationToken ct = default);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
