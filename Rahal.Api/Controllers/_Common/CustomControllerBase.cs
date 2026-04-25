using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Rahal.Api.Controllers._Common
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("per-user")]
    public class CustomControllerBase : ControllerBase
    {
        // Helper to get current user ID
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        // Helper to get current user email
        protected string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        // Helper to get current user roles
        protected List<string> GetCurrentUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
    }
}
