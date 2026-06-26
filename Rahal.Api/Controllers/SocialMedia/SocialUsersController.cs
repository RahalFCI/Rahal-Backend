using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using SocialMedia.Application.DTOs.Users;
using SocialMedia.Application.Interfaces;

namespace Rahal.Api.Controllers.SocialMedia
{
    [ApiController]
    [Route("api/social-media/users")]
    [EnableRateLimiting("per-user")]
    public class SocialUsersController : ControllerBase
    {
        private readonly IFollowService _followService;

        public SocialUsersController(IFollowService followService)
        {
            _followService = followService;
        }

        /// <summary>
        /// Gets social media users with follower and following counters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SocialUserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSocialUsersAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _followService.GetSocialUsersAsync(
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Gets a social media user with follower and following counters.
        /// </summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SocialUserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SocialUserResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<SocialUserResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSocialUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _followService.GetSocialUserByIdAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.errorCode == ErrorCode.NotFound
                    ? NotFound(result)
                    : BadRequest(result);
            }

            return Ok(result);
        }
    }
}
