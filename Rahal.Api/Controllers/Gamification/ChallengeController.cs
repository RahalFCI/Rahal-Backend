using Gamification.Application.CQRS.Commands.Challenge;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Shared.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Gamification
{
    public class ChallengeController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public ChallengeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChallengeAsync(
            [FromBody] CreateChallengeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateChallengeCommand(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChallengeAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateChallengeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateChallengeCommand(id, dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChallengeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteChallengeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PermanentDeleteChallengeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PermenantDeleteChallengeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreDeletedChallengeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RestoreDeletedChallengeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChallengeByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetChallengeByIdQuery(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("name/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChallengeByNameAsync(
            [FromRoute] string name,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetChallengeByNameQuery(name),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllChallengesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllChallengesQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("place/{placeId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChallengesByPlaceIdAsync(
            [FromRoute] Guid placeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetChallengesByPlaceIdQuery(placeId, request),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
