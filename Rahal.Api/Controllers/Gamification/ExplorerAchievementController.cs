using Gamification.Application.CQRS.Commands.ExplorerAchievement;
using Gamification.Application.CQRS.Orchestrators.ExplorerAchievements;
using Gamification.Application.CQRS.Queries.ExplorerAchievement;
using Gamification.Application.DTOs.ExplorerAchievement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Gamification
{
    public class ExplorerAchievementController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public ExplorerAchievementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Explorer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateExplorerAchievementAsync(
            [FromBody] CreateExplorerAchievementDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateExplorerAchievementWithItsRewardOrchestrator(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Explorer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteExplorerAchievementAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteExplorerAchievementCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PermanentDeleteExplorerAchievementAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new PermanentDeleteExplorerAchievementCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin,Explorer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreDeletedExplorerAchievementAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RestoreDeletedExplorerAchievementCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExplorerAchievementByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetExplorerAchievementByIdQuery(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllExplorerAchievementsAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllExplorerAchievementsQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("explorer/{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExplorerAchievementsByExplorerIdAsync(
            [FromRoute] Guid explorerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetExplorerAchievementsByExplorerIdQuery(explorerId, request),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("achievement/{achievementId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExplorerAchievementsByAchievementIdAsync(
            [FromRoute] Guid achievementId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetExplorerAchievementsByAchievementIdQuery(achievementId, request),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
