using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Application.DTOs;
using SocialMedia.Application.DTOs.Posts;
using SocialMedia.Application.Interfaces;
using System.Security.Claims;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/users/{userId:guid}/posts")]
    [EnableRateLimiting("per-user")]
    public class UserPostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public UserPostsController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Gets a user's profile posts using database cursor pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<FeedPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserPostsPaginatedAsync(
            Guid userId,
            [FromQuery] long? cursor,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var viewerUserId = GetCurrentUserId();
            var result = await _postService.GetUserPostsPaginatedAsync(
                userId,
                viewerUserId,
                cursor,
                limit,
                cancellationToken);

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
