using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Places.Application.DTOs.Place;
using Places.Application.Interfaces;
using Places.Application.DTOs.Location;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Places
{
    public class PlaceController : CustomControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlaceController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _placeService.GetPlaceByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _placeService.GetAllPlacesAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCategoryAsync([FromRoute] Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _placeService.GetPlacesByCategoryIdAsync(categoryId, request, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePlaceDto createPlaceDto, CancellationToken cancellationToken)
        {
            var result = await _placeService.CreatePlaceAsync(createPlaceDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdatePlaceDto updatePlaceDto, CancellationToken cancellationToken)
        {
            var result = await _placeService.UpdatePlaceAsync(id, updatePlaceDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _placeService.DeletePlaceAsync(id, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("search")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByLocationAsync([FromQuery] LocationSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _placeService.SearchPlacesByLocationAsync(request.Latitude, request.Longitude, request.RadiusInMeters, request.offsetPaginationRequest, request.CategoryId, cancellationToken);
            return Ok(result);
        }
    }

    
}
