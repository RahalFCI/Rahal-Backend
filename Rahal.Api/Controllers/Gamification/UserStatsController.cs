using Gamification.Application.CQRS.Queries.UserStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Gamification
{
    public class UserStatsController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public UserStatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUserStatsAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetAllUserStatsQuery(request),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserStatsByExplorerIdAsync(
            [FromRoute] Guid explorerId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetUserStatsByExplorerIdQuery(explorerId),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
