using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SocialMedia.Application.Interfaces;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/users/{userId:guid}/feed")]
    [EnableRateLimiting("per-user")]
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
        [Authorize]
        [ProducesResponseType(typeof(Shared.Application.DTOs.ApiResponse<global::SocialMedia.Application.DTOs.Posts.FeedPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFeedPaginatedAsync(
            Guid userId,
            [FromQuery] long? cursor,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _postService.GetFeedPaginatedAsync(userId, cursor, limit, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
