using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System.Net;

namespace Rahal.Api.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IRateLimitService rateLimitService, ICurrentUserService currentUserService)
        {
            // Get client identifier
            var identifier = GetClientIdentifier(context, currentUserService);

            // Check rate limit
            var result = await rateLimitService.CheckRateLimitAsync(identifier);

            // Add rate limit headers
            AddRateLimitHeaders(context, result);

            if (!result.IsAllowed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. You have exceeded the limit of {result.Limit} requests per window.",
                    limit = result.Limit,
                    current = result.CurrentCount,
                    windowResetTime = result.WindowResetTime,
                    retryAfter = result.RetryAfterSeconds
                });
                return;
            }

            await _next(context);
        }

        private void AddRateLimitHeaders(HttpContext context, RateLimitResult result)
        {
            context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] =
                new DateTimeOffset(result.WindowResetTime).ToUnixTimeSeconds().ToString();

            if (!result.IsAllowed)
            {
                context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();
            }
        }

        private string GetClientIdentifier(HttpContext context, ICurrentUserService currentUserService)
        {
            var userId = currentUserService.UserId;

            if (!string.IsNullOrEmpty(userId))
                return $"user:{userId}";

            // Fall back to IP address
            var ipAddress = GetClientIpAddress(context);
            return $"ip:{ipAddress}";
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for X-Forwarded-For header (proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                return ips[0].Trim();
            }

            // Check for X-Real-IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            // Fall back to RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
