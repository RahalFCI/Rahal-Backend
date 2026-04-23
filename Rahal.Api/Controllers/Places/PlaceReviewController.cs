using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Places.Application.DTOs.PlaceReview;
using Places.Application.Interfaces;

namespace Rahal.Api.Controllers.Places
{
    public class PlaceReviewController : CustomControllerBase
    {
        private readonly IPlaceReviewService _reviewService;

        public PlaceReviewController(IPlaceReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{explorerId}/{placeId}/{checkInId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, [FromRoute] Guid checkInId, CancellationToken cancellationToken)
        {
            var result = await _reviewService.GetPlaceReviewAsync(explorerId, placeId, checkInId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("place/{placeId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewsByPlaceAsync([FromRoute] Guid placeId, CancellationToken cancellationToken)
        {
            var result = await _reviewService.GetReviewsByPlaceIdAsync(placeId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("verified/{placeId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVerifiedReviewsByPlaceAsync([FromRoute] Guid placeId, CancellationToken cancellationToken)
        {
            var result = await _reviewService.GetVerifiedReviewsByPlaceIdAsync(placeId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("explorer/{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewsByExplorerAsync([FromRoute] Guid explorerId, CancellationToken cancellationToken)
        {
            var result = await _reviewService.GetReviewsByExplorerIdAsync(explorerId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Explorer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePlaceReviewDto createReviewDto, CancellationToken cancellationToken)
        {
            var result = await _reviewService.CreatePlaceReviewAsync(createReviewDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{explorerId}/{placeId}/{checkInId}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, [FromRoute] Guid checkInId, [FromBody] UpdatePlaceReviewDto updateReviewDto, CancellationToken cancellationToken)
        {
            var result = await _reviewService.UpdatePlaceReviewAsync(explorerId, placeId, checkInId, updateReviewDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{explorerId}/{placeId}/{checkInId}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid explorerId, [FromRoute] Guid placeId, [FromRoute] Guid checkInId, CancellationToken cancellationToken)
        {
            var result = await _reviewService.DeletePlaceReviewAsync(explorerId, placeId, checkInId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
