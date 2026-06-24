using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using SocialMedia.Application.DTOs.Posts;
using SocialMedia.Application.Interfaces;

namespace Rahal.Api.Controllers.SocialMedia
{
    public class PostController : CustomControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Creates a new post. Media public_ids must have been pre-signed via
        /// POST /api/media/signatures. Any unrecognised ID is rejected.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePostAsync(
            [FromBody] CreatePostRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _postService.CreatePostAsync(request, userId, cancellationToken);

            if (!result.IsSuccess)
                return result.errorCode == Shared.Domain.Enums.ErrorCode.Unauthorized
                    ? Unauthorized(result)
                    : BadRequest(result);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Likes a post. Returns 400 if the user has already liked it.
        /// The cache is hydrated on miss; only the Redis LikesCount is updated here —
        /// the DB write is handled by the same request for consistency.
        /// </summary>
        [HttpPost("{postId:guid}/like")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LikePostAsync(
            Guid postId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _postService.LikePostAsync(postId, userId, cancellationToken);

            if (!result.IsSuccess)
                return result.errorCode == Shared.Domain.Enums.ErrorCode.NotFound
                    ? NotFound(result)
                    : BadRequest(result);

            return Ok(result);
        }
    }
}
