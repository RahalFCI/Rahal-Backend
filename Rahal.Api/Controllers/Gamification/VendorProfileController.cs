using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.DTOs.Vendor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Gamification
{
    public class VendorProfileController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public VendorProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateVendorProfileAsync(
            [FromForm] AddVendorDto dto,
            [FromForm] IFormFile? profilePicture,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateVendorProfileOrchestrator(dto, profilePicture),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{vendorId}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveVendorProfileAsync(
            [FromRoute] Guid vendorId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ApproveVendorProfileCommand(vendorId),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }



        [HttpPut("{vendorId}")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVendorProfileAsync(
            [FromRoute] Guid vendorId,
            [FromForm] UpdateVendorDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateVendorProfileCommand(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpPut("{vendorId}/update-picture")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVendorProfilePictureAsync(
            [FromRoute] Guid vendorId,
            [FromForm] IFormFile profilePicture,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateVendorProfilePictureOrchestrator(vendorId, profilePicture),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpGet("{vendorId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVendorProfileByIdAsync(
            [FromRoute] Guid vendorId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetVendorProfileByIdQuery(vendorId),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllVendorProfilesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllVendorsProfilesQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("unapproved")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnapprovedVendorProfilesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetUnapprovedVendorProfilesQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVendorProfilesIncludingDeletedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetVendorProfilesIncludingDeletedQuery(request),
                cancellationToken);
            return Ok(result);
        }
    }
}
