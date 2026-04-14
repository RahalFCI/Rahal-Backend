using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Facade interface for orchestrating Google OAuth authentication
    /// </summary>
    public interface IOAuthGoogleFacade
    {
        Task<ApiResponse<AuthResponseDto>> AuthenticateAsync(
            string idToken,
            UserRoleEnum userType,
            CancellationToken ct = default);
    }
}
