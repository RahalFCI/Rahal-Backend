using Gamification.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rahal.Api.Attributes;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System.Security.Claims;

namespace Rahal.Api.Filters
{
    public class ProfileSetupRequiredFilter : IAsyncActionFilter
    {
        private readonly IProfileChecker _profileChecker;

        public ProfileSetupRequiredFilter(IProfileChecker profileChecker)
        {
            _profileChecker = profileChecker;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            var skipProfileCheck =
                endpoint?.Metadata.GetMetadata<SkipProfileCheckAttribute>() != null;
            var allowAnonymous =
                endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

            if (skipProfileCheck || allowAnonymous)
            {
                await next();
                return;
            }

            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                await next();
                return;
            }

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = user.FindFirstValue(ClaimTypes.Role);

            var hasProfile = await _profileChecker.HasProfileAsync(userId, role);

            if (!hasProfile)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Failure(ErrorCode.ProfileSetupRequired))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
