using Gamification.Application.CQRS.Commands.VendorCategories;
using Gamification.Application.CQRS.Queries.VendorCategories;
using Gamification.Application.DTOs.VendorCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Gamification
{
    public class VendorCategoryController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public VendorCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateVendorCategoryAsync(
            [FromBody] string CategoryName,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateVendorCategoryCommand(CategoryName),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVendorCategoryAsync(
            [FromRoute] Guid id,
            [FromBody] string CategoryName,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateVendorCategoryCommand(id, CategoryName),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVendorCategoryAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteVendorCategoryCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVendorCategoryByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetVendorCategoryByIdQuery(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("name/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVendorCategoryByNameAsync(
            [FromRoute] string name,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetVendorCategoryByNameQuery(name),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllVendorCategoriesAsync(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllVendorCategoriesQuery(),
                cancellationToken);
            return Ok(result);
        }
    }
}
