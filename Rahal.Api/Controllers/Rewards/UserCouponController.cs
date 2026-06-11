using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Rewards.Application.DTOs.UserCoupons;
using Rewards.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Rewards
{
    public class UserCouponController : CustomControllerBase
    {
        private readonly IUserCouponService _userCouponService;

        public UserCouponController(IUserCouponService userCouponService)
        {
            _userCouponService = userCouponService;
        }

        [HttpPost("claim/{couponId}")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> ClaimAsync([FromRoute] Guid couponId, CancellationToken cancellationToken)
        {
            var result = await _userCouponService.ClaimAsync(GetCurrentUserId(), couponId, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("redeem")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> RedeemAsync([FromBody] RedeemCouponDto dto, CancellationToken cancellationToken)
        {
            dto.VendorId = GetCurrentUserId();
            var result = await _userCouponService.RedeemAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpGet("code/{code}")]
        [Authorize(Roles = "Vendor,Admin")]
        public async Task<IActionResult> GetByCodeAsync([FromRoute] string code, CancellationToken cancellationToken)
        {
            var result = await _userCouponService.GetByCodeAsync(code, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> GetMineAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _userCouponService.GetByExplorerAsync(
                GetCurrentUserId(),
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);
            return Ok(result);
        }
    }
}
