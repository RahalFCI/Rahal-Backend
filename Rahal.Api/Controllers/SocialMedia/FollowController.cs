using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Domain.Enums;
using SocialMedia.Application.Interfaces;
using System.Security.Claims;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/users")]
    [EnableRateLimiting("per-user")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        /// <summary>
        /// Follows a user. Returns 400 if the current user already follows the target user.
        /// </summary>
        [HttpPost("{targetUserId:guid}/follow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> FollowAsync(
            Guid targetUserId,
            CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _followService.FollowAsync(currentUserId, targetUserId, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.errorCode == ErrorCode.DatabaseError
                    ? StatusCode(StatusCodes.Status500InternalServerError, result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Unfollows a user. Returns 400 if the current user does not follow the target user.
        /// </summary>
        [HttpDelete("{targetUserId:guid}/follow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnfollowAsync(
            Guid targetUserId,
            CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _followService.UnfollowAsync(currentUserId, targetUserId, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.errorCode == ErrorCode.DatabaseError
                    ? StatusCode(StatusCodes.Status500InternalServerError, result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
