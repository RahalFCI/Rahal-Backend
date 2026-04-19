using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System.Security.Claims;
using Users.Domain.Entities._Common;

namespace Rahal.Api.Filters
{
    /// <summary>
    /// Action filter that enforces email verification by checking:
    /// 1. JWT claims for email_verified claim (fast, stateless)
    /// 2. Database fallback if claim not found (safety check)
    /// 3. Rejects request if neither check passes
    /// 
    /// Usage: Apply [RequireEmailVerified] attribute to controller or action
    /// 
    /// Response Codes:
    /// - 200 OK: Email is verified (claim or database)
    /// - 403 Forbidden: Email is NOT verified
    /// - 500 Internal Server Error: Error checking database
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireEmailVerifiedAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new UnauthorizedObjectResult(
                    ApiResponse<string>.Failure(ErrorCode.InvalidCredentials));
                return;
            }

            // Step 1: Check JWT claims for email_verified
            var emailVerifiedClaim = context.HttpContext.User.FindFirstValue("email_verified");

            if (!string.IsNullOrEmpty(emailVerifiedClaim) && 
                bool.TryParse(emailVerifiedClaim, out var isVerified) && 
                isVerified)
            {
                // Claim exists and is true - allow request
                await next();
                return;
            }

            // Step 2: Fallback - check database
            try
            {
                var userIdClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Result = new UnauthorizedObjectResult(
                        ApiResponse<string>.Failure(ErrorCode.InvalidCredentials));
                    return;
                }

                // Get UserManager from DI
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var user = await userManager.FindByIdAsync(userId.ToString());

                if (user?.EmailConfirmed == true)
                {
                    // Database confirms email is verified - allow request
                    await next();
                    return;
                }

                // Email not verified (neither claim nor database)
                context.Result = new ForbidResult();
            }
            catch (Exception ex)
            {
                // Log error and return 500
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequireEmailVerifiedAttribute>>();
                logger.LogError(ex, "Error checking email verification status for user");

                context.Result = new ObjectResult(
                    ApiResponse<string>.Failure(ErrorCode.UnknownError))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
