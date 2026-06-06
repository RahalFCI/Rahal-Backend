using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.CheckInChallenges.Queries
{
    public class GetCheckInChallengesByCheckInIdQueryHandler : IRequestHandler<GetCheckInChallengesByCheckInIdQuery, ApiResponse<PagedResult<GetCheckInChallengeDto>>>
    {
        private readonly IGamificationRepository<CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByCheckInIdQueryHandler> _logger;

        public GetCheckInChallengesByCheckInIdQueryHandler(
            IGamificationRepository<CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByCheckInIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetCheckInChallengeDto>>> Handle(GetCheckInChallengesByCheckInIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for check-in {CheckInId} - page {Page}, pageSize {PageSize}", request.CheckInId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(c => c.CheckInId == request.CheckInId)
                .Include(c => c.Challenge)
                .Select(c => CheckInChallengeMapper.ToGetDto(c))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} check-in challenges for check-in {CheckInId} out of {TotalCount}",
                result.Items.Count(), request.CheckInId, result.TotalCount);

            return ApiResponse<PagedResult<GetCheckInChallengeDto>>.Success(result);
        }
    }
}
