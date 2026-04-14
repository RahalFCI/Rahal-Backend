using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.OAuth;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Interface for OAuth authentication service
    /// Handles OAuth signin and registration flows
    /// </summary>
    public interface IOAuthGoogleService
    {
        Task<ApiResponse<AuthResponseDto>> SignInAsync(User user, GoogleUserInfo googleUserInfo, CancellationToken ct = default);
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(User user, GoogleUserInfo googleUserInfo, CancellationToken ct = default);
    }
}
