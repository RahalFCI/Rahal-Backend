using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Rewards.Application.DTOs.TravelPlans;
using Rewards.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Rewards
{
    public class TravelPlanController : CustomControllerBase
    {
        private readonly ITravelPlanService _travelPlanService;

        public TravelPlanController(ITravelPlanService travelPlanService)
        {
            _travelPlanService = travelPlanService;
        }

        [HttpPost]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTravelPlanDto dto, CancellationToken cancellationToken)
        {
            var result = await _travelPlanService.CreateAsync(GetCurrentUserId(), dto, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _travelPlanService.GetByIdAsync(GetCurrentUserId(), id, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Explorer")]
        public async Task<IActionResult> GetMineAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _travelPlanService.GetByExplorerAsync(
                GetCurrentUserId(),
                new OffsetPaginationRequest { Page = page, PageSize = pageSize },
                cancellationToken);
            return Ok(result);
        }
    }
}
