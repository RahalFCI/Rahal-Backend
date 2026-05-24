using Gamification.Application.CQRS.Queries.XpTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Application.Pagination;

namespace Rahal.Api.Controllers.Gamification
{
    public class XpTransactionController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public XpTransactionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("explorer/{explorerId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetXpTransactionsByExplorerIdAsync(
            [FromRoute] Guid explorerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new OffsetPaginationRequest { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(
                new GetXpTransactionsByExplorerIdQuery(explorerId, request),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}

