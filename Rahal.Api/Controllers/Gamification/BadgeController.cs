using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.DTOs.Badge;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Shared.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Gamification
{
    public class BadgeController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public BadgeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBadgeAsync(
            [FromBody] CreateBadgeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateBadgeCommand(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBadgeAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateBadgeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateBadgeCommand(id, dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBadgeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteBadgeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PermanentDeleteBadgeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PermenantDeleteBadgeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreDeletedBadgeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RestoreDeletedBadgeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBadgeByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetBadgeByIdQuery(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("name/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBadgeByNameAsync(
            [FromRoute] string name,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetBadgeByNameQuery(name),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllBadgesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllBadgesQuery(request),
                cancellationToken);
            return Ok(result);
        }
    }
}
