using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Rewards.Application.DTOs.Subscriptions;
using Rewards.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Rewards
{
    public class SubscriptionController : CustomControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("purchase")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> PurchaseAsync([FromBody] PurchaseSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.PurchaseAsync(GetCurrentUserId(), dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> GetActiveAsync(CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.GetActiveAsync(GetCurrentUserId(), cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> GetMineAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _subscriptionService.GetByExplorerAsync(
                GetCurrentUserId(),
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);
            return Ok(result);
        }

        [HttpPut("cancel/{userId}")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> CancelAsync([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.CancelAsync(userId, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }
    }
}
