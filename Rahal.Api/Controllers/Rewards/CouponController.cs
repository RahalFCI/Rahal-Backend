using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Rewards.Application.DTOs.Coupons;
using Rewards.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Rewards
{
    public class CouponController : CustomControllerBase
    {
        private readonly ICouponService _couponService;
        private readonly IUserCouponService _userCouponService;

        public CouponController(ICouponService couponService, IUserCouponService userCouponService)
        {
            _couponService = couponService;
            _userCouponService = userCouponService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
                dto.VendorId = GetCurrentUserId();

            var result = await _couponService.CreateAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateCouponDto dto, CancellationToken cancellationToken)
        {
            var result = await _couponService.UpdateAsync(id, dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _couponService.DeleteAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _couponService.GetByIdAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _couponService.GetAllAsync(new OffsetPaginationRequest { Page = page, PageSize = pageSize }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendor/mine")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMineAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _couponService.GetByVendorIdAsync(GetCurrentUserId(), new OffsetPaginationRequest { Page = page, PageSize = pageSize }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendor/{vendorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByVendorAsync([FromRoute] Guid vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _couponService.GetByVendorIdAsync(vendorId, new OffsetPaginationRequest { Page = page, PageSize = pageSize }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}/redemptions")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> GetRedemptionsAsync([FromRoute] Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var vendorId = User.IsInRole("Vendor") && !User.IsInRole("Admin") ? GetCurrentUserId() : (Guid?)null;
            var result = await _userCouponService.GetByCouponAsync(id, vendorId, new OffsetPaginationRequest { Page = page, PageSize = pageSize }, cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpGet("{id}/stats")]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> GetStatsAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var vendorId = User.IsInRole("Vendor") && !User.IsInRole("Admin") ? GetCurrentUserId() : (Guid?)null;
            var result = await _userCouponService.GetStatsByCouponAsync(id, vendorId, cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchAsync([FromQuery] CouponSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _couponService.SearchAsync(request, cancellationToken);
            return Ok(result);
        }

        private IActionResult ToActionResult<T>(Shared.Application.DTOs.ApiResponse<T> response)
        {
            return response.errorCode switch
            {
                ErrorCode.NotFound => NotFound(response),
                ErrorCode.Forbidden => Forbid(),
                _ => BadRequest(response)
            };
        }
    }
}
