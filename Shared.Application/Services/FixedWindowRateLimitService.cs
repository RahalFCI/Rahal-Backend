using Microsoft.Extensions.Options;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Services
{
    public class FixedWindowRateLimitService : IRateLimitService
    {
        private readonly ICacheService _cache;
        private readonly RateLimitSettings _settings;

        public FixedWindowRateLimitService(ICacheService cache, IOptions<RateLimitSettings> settings)
        {
            _cache = cache;
            _settings = settings.Value;
        }

        public async Task<RateLimitResult> CheckRateLimitAsync(string identifier)
        {
            int customLimit = _settings.RequestLimit;
            int customWindowSeconds = _settings.WindowInSeconds;

            // Create window-based key using current timestamp
            var windowStart = GetCurrentWindowStart(customWindowSeconds);
            var key = $"rate_limit:{identifier}:{windowStart}";
            var expiration = TimeSpan.FromSeconds(customWindowSeconds);

            // Increment counter for this window
            var currentCount = await _cache.IncrementAsync(key, expiration);

            // Calculate remaining requests
            var remaining = Math.Max(0, customLimit - (int)currentCount);
            var isAllowed = currentCount <= customLimit;

            // Calculate window reset time
            var windowResetTime = DateTimeOffset
                .FromUnixTimeSeconds(windowStart)
                .AddSeconds(customWindowSeconds)
                .UtcDateTime;

            var retryAfterSeconds = (int)(windowResetTime - DateTime.UtcNow).TotalSeconds;

            return new RateLimitResult
            {
                IsAllowed = isAllowed,
                Limit = customLimit,
                Remaining = remaining,
                CurrentCount = (int)currentCount,
                WindowResetTime = windowResetTime,
                RetryAfterSeconds = Math.Max(0, retryAfterSeconds)
            };

        }

        private long GetCurrentWindowStart(int windowSeconds)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return (now / windowSeconds) * windowSeconds; //Calculation to drop the decimal after division so every request within the same window seconds will be rounded to the same number
        }
    }
}
