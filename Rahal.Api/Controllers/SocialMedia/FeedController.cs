using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SocialMedia.Application.Interfaces;
using System.Security.Claims;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/users/{userId:guid}/feed")]
    [EnableRateLimiting("per-user")]
    [Authorize(Roles = "Explorer")]
    public class FeedController : ControllerBase
    {
        private readonly IPostService _postService;

        public FeedController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Gets a user's feed using Redis ZSET pagination with database fallback.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Shared.Application.DTOs.ApiResponse<global::SocialMedia.Application.DTOs.Posts.FeedPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFeedPaginatedAsync(
            Guid userId,
            [FromQuery] long? cursor,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (currentUserId != userId)
            {
                return Forbid();
            }

            var result = await _postService.GetFeedPaginatedAsync(userId, cursor, limit, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
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
