using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.OAuth;

namespace Users.Application.Interfaces
{

    public interface IGoogleTokenValidator
    {
        Task<ApiResponse<GoogleUserInfo>> ValidateAsync(string idToken, CancellationToken ct = default);
    }

}
