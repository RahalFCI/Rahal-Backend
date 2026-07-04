using Gamification.Application.CQRS.Commands.VendorBranches;
using Gamification.Application.CQRS.Queries.VendorBranches;
using Gamification.Application.DTOs.VendorBranches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Gamification
{
    public class VendorBranchController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public VendorBranchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateVendorBranchDto dto,
            CancellationToken cancellationToken)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
                dto.VendorId = GetCurrentUserId();

            var result = await _mediator.Send(new CreateVendorBranchCommand(dto), cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetVendorBranchByIdQuery(id), cancellationToken);
            if (result.IsSuccess && User.IsInRole("Vendor") && !User.IsInRole("Admin") && result.Data.VendorId != GetCurrentUserId())
                return Forbid();

            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpGet("vendor/{vendorId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByVendorIdAsync(
            [FromRoute] Guid vendorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin") && vendorId != GetCurrentUserId())
                return Forbid();

            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(new GetVendorBranchesByVendorIdQuery(vendorId, request), cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateVendorBranchDto dto,
            CancellationToken cancellationToken)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                var existing = await _mediator.Send(new GetVendorBranchByIdQuery(id), cancellationToken);
                if (!existing.IsSuccess)
                    return ToActionResult(existing);
                if (existing.Data.VendorId != GetCurrentUserId())
                    return Forbid();
            }

            var result = await _mediator.Send(new UpdateVendorBranchCommand(id, dto), cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            if (User.IsInRole("Vendor") && !User.IsInRole("Admin"))
            {
                var existing = await _mediator.Send(new GetVendorBranchByIdQuery(id), cancellationToken);
                if (!existing.IsSuccess)
                    return ToActionResult(existing);
                if (existing.Data.VendorId != GetCurrentUserId())
                    return Forbid();
            }

            var result = await _mediator.Send(new DeleteVendorBranchCommand(id), cancellationToken);
            return result.IsSuccess ? Ok(result) : ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(ApiResponse<T> response)
        {
            return response.errorCode switch
            {
                ErrorCode.NotFound => NotFound(response),
                ErrorCode.Forbidden => Forbid(),
                ErrorCode.AlreadyExists or ErrorCode.Conflict => Conflict(response),
                ErrorCode.Timeout or ErrorCode.ExternalServiceError => StatusCode(StatusCodes.Status502BadGateway, response),
                _ => BadRequest(response)
            };
        }
    }
}
