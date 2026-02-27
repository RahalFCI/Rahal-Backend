using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IRateLimitService
    {
        Task<RateLimitResult> CheckRateLimitAsync(string identifier);
    }
}
