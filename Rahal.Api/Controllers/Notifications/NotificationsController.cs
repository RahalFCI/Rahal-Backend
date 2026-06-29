using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Notifications
{
    [Route("api/notifications")]
    [EnableRateLimiting("per-user")]
    [Authorize]
    public class NotificationsController : CustomControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets the authenticated user's unread notification count.
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets the authenticated user's notifications using CreatedAt keyset pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<NotificationsPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserNotificationsPaginatedAsync(
            [FromQuery] DateTime? cursor,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetUserNotificationsPaginatedAsync(
                userId,
                cursor,
                limit,
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Marks one notification as read.
        /// </summary>
        [HttpPatch("{notificationId:guid}/read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.MarkAsReadAsync(
                userId,
                notificationId,
                cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.errorCode == ErrorCode.NotFound)
                {
                    return NotFound(result);
                }

                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Marks all authenticated-user notifications as read.
        /// </summary>
        [HttpPatch("read-all")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.MarkAsReadAsync(
                userId,
                notificationId: null,
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Stores or replaces the authenticated user's Firebase Cloud Messaging token.
        /// </summary>
        [HttpPost("fcm-token")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetFcmTokenAsync(
            [FromBody] SetFcmTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.SetFcmTokenAsync(
                userId,
                request.Token,
                cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.errorCode == ErrorCode.ValidationError)
                {
                    return BadRequest(result);
                }

                return Unauthorized(result);
            }

            return Ok(result);
        }
    }
}
