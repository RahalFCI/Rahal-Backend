using Gamification.Application.CQRS.Queries.CheckInChallenge;
using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Application.Mappers;
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
    public class GetCheckInChallengesByChallengeIdQueryHandler : IRequestHandler<GetCheckInChallengesByChallengeIdQuery, ApiResponse<PagedResult<GetCheckInChallengeDto>>>
    {
        private readonly IGamificationRepository<Domain.Entities.CheckInChallenge> _repository;
        private readonly ILogger<GetCheckInChallengesByChallengeIdQueryHandler> _logger;

        public GetCheckInChallengesByChallengeIdQueryHandler(
            IGamificationRepository<Domain.Entities.CheckInChallenge> repository,
            ILogger<GetCheckInChallengesByChallengeIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetCheckInChallengeDto>>> Handle(GetCheckInChallengesByChallengeIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching check-in challenges for challenge {ChallengeId} - page {Page}, pageSize {PageSize}", request.ChallengeId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(c => c.ChallengeId == request.ChallengeId)
                .Include(c => c.Challenge)
                .Select(c => CheckInChallengeMapper.ToGetDto(c))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} check-in challenges for challenge {ChallengeId} out of {TotalCount}",
                result.Items.Count(), request.ChallengeId, result.TotalCount);

            return ApiResponse<PagedResult<GetCheckInChallengeDto>>.Success(result);
        }
    }
}
