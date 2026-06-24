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

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
        {
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

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchAsync([FromQuery] CouponSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _couponService.SearchAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
