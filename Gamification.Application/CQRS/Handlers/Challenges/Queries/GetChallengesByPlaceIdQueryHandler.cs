using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.DTOs.Challenge;
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

namespace Gamification.Application.CQRS.Handlers.Challenges.Queries
{
    public class GetChallengesByPlaceIdQueryHandler : IRequestHandler<GetChallengesByPlaceIdQuery, ApiResponse<PagedResult<GetChallengeDto>>>
    {
        private readonly IGenericRepository<Challenge> _repository;
        private readonly ILogger<GetChallengesByPlaceIdQueryHandler> _logger;

        public GetChallengesByPlaceIdQueryHandler(
            IGenericRepository<Challenge> repository,
            ILogger<GetChallengesByPlaceIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetChallengeDto>>> Handle(GetChallengesByPlaceIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching challenges for place {PlaceId} - page {Page}, pageSize {PageSize}", request.PlaceId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(c => c.PlaceId == request.PlaceId)
                .Select(c => ChallengeMapper.ToGetDto(c))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} challenges for place {PlaceId} out of {TotalCount}", result.Items.Count(), request.PlaceId, result.TotalCount);

            return ApiResponse<PagedResult<GetChallengeDto>>.Success(result);
        }
    }
}
