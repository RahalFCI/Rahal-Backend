using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Attributes;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Gamification
{
    public class ExplorerProfileController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public ExplorerProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [SkipProfileCheckAttribute]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateExplorerProfileAsync(
            [FromForm] AddExplorerDto dto,
            [FromForm] IFormFile? profilePicture,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateExplorerProfileWithUserStatsOrchestrator(dto, profilePicture),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{explorerId}/update-picture")]
        [Authorize(Roles = "Explorer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateExplorerProfilePictureAsync(
            [FromRoute] Guid explorerId,
            [FromForm] IFormFile profilePicture,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateExplorerProfilePictureOrchestrator(explorerId, profilePicture),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpPut("{explorerId}")]
        [Authorize(Roles = "Explorer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateExplorerProfileAsync(
            [FromRoute] Guid explorerId,
            [FromForm] UpdateExplorerDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateExplorerProfileCommand(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpGet("{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExplorerProfileByIdAsync(
            [FromRoute] Guid explorerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetExplorerProfileByIdQuery(explorerId),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllExplorerProfilesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllExplorerProfilesQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExplorerProfilesIncludingDeletedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetExplorerProfilesIncludingDeletedQuery(request),
                cancellationToken);
            return Ok(result);
        }
    }
}
