using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Users;
using SocialMedia.Application.Interfaces;
using System.Security.Claims;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/users")]
    [EnableRateLimiting("per-user")]
    [Authorize(Roles = "Explorer")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        /// <summary>
        /// Gets users who follow the requested user.
        /// </summary>
        [HttpGet("{userId:guid}/followers")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SocialUserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFollowersAsync(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _followService.GetFollowersAsync(
                userId,
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets users followed by the requested user.
        /// </summary>
        [HttpGet("{userId:guid}/followees")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SocialUserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFolloweesAsync(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _followService.GetFolloweesAsync(
                userId,
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Follows a user. Returns 400 if the current user follows themselves or already follows the target user.
        /// </summary>
        [HttpPost("{targetUserId:guid}/follow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FollowAsync(
            Guid targetUserId,
            CancellationToken cancellationToken)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _followService.FollowAsync(currentUserId, targetUserId, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.errorCode == ErrorCode.DatabaseError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return result.errorCode == ErrorCode.NotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Unfollows a user. Returns 400 if the current user does not follow the target user.
        /// </summary>
        [HttpDelete("{targetUserId:guid}/follow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
