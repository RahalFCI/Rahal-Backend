using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Places.Application.DTOs.PlacePhoto;
using Places.Application.Interfaces;

namespace Rahal.Api.Controllers.Places
{
    public class PlacePhotoController : CustomControllerBase
    {
        private readonly IPlacePhotoService _photoService;

        public PlacePhotoController(IPlacePhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet("place/{placeId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotosByPlaceAsync([FromRoute] Guid placeId, CancellationToken cancellationToken)
        {
            var result = await _photoService.GetPhotosByPlaceIdAsync(placeId, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPhotoAsync([FromForm] Guid PlaceId, [FromForm] IFormFile Photo, CancellationToken cancellationToken)
        {
            var result = await _photoService.AddPlacePhotoAsync(PlaceId, Photo, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("place/{placeId}/url")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhotoAsync([FromRoute] Guid placeId, [FromQuery] string url, CancellationToken cancellationToken)
        {
            var result = await _photoService.DeletePlacePhotoAsync(placeId, url, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("batch")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotosByPlacesAsync([FromBody] IEnumerable<Guid> placeIds, CancellationToken cancellationToken)
        {
            var result = await _photoService.GetPhotosByPlaceIdsAsync(placeIds, cancellationToken);
            return Ok(result);
        }
    }
}
