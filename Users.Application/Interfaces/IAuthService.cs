using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;
using Shared.Application.DTOs;
using Users.Domain.Entities._Common;
using Users.Application.DTOs.Auth;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Single instance service for authentication
    /// Handles login, logout, and registration for all user types
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto?>> RegisterAsync(User user, string Password, IFormFile? profilePicture = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<AuthResponseDto?>> LoginAsync(AuthRequestDto loginRequestDto, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}
