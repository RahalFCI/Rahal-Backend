using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Places.Application.DTOs.CheckIn;
using Places.Application.Interfaces;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Places
{
    public class CheckInController : CustomControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [HttpGet("{explorerId}/{placeId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCheckInAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, CancellationToken cancellationToken)
        {
            var result = await _checkInService.GetCheckInAsync(explorerId, placeId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCheckInsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _checkInService.GetAllCheckInAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("place/{placeId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCheckInsByPlaceAsync([FromRoute] Guid placeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _checkInService.GetCheckInsByPlaceIdAsync(placeId, request, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("explorer/{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCheckInsByExplorerAsync([FromRoute] Guid explorerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _checkInService.GetCheckInsByExplorerIdAsync(explorerId, request, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{explorerId}")]
        [Authorize(Roles = "Explorer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAsync([FromBody] CheckInRequestDto createCheckInDto, CancellationToken cancellationToken)
        {
            var explorerId = GetCurrentUserId();

            var result = await _checkInService.CheckInAsync(explorerId, createCheckInDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{explorerId}/{placeId}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, [FromBody] UpdateCheckInDto updateCheckInDto, CancellationToken cancellationToken)
        {
            if (GetCurrentUserRoles().Contains("Explorer") && explorerId != GetCurrentUserId())
                return Forbid();

            var result = await _checkInService.UpdateCheckInStatusAsync(explorerId, placeId, updateCheckInDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{explorerId}/{placeId}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, CancellationToken cancellationToken)
        {
            if (GetCurrentUserRoles().Contains("Explorer") && explorerId != GetCurrentUserId())
                return Forbid();

            var result = await _checkInService.DeleteCheckInAsync(explorerId, placeId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _checkInService.GetPendingCheckInsAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
