using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Application.CQRS.Queries.AchievementCriteriaTypes;
using Gamification.Application.DTOs.AchievementCriteriaType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rahal.Api.Controllers._Common;
using Shared.Domain.Enums;

namespace Rahal.Api.Controllers.Gamification
{
    public class AchievementCriteriaTypeController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public AchievementCriteriaTypeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAchievementCriteriaTypeAsync(
            [FromBody] AddAchievementCriteriaTypeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateAchievementCriteriaTypeCommand(dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAchievementCriteriaTypeAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateAchievementCriteriaTypeDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateAchievementCriteriaTypeCommand(id, dto),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : result.errorCode == ErrorCode.NotFound ? NotFound(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAchievementCriteriaTypeAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteAchievementCriteriaTypeCommand(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAchievementCriteriaTypeByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAchievementCriteriaTypeByIdQuery(id),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("name/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAchievementCriteriaTypeByNameAsync(
            [FromRoute] string name,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAchievementCriteriaTypeByNameQuery(name),
                cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAchievementCriteriaTypesAsync(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllAchievementCriteriaTypesQuery(),
                cancellationToken);
            return Ok(result);
        }
    }
}
