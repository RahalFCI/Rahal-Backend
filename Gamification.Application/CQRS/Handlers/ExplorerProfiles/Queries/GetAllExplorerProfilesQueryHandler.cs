using Gamification.Application.CQRS.Handlers.Challenges.Queries;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetAllExplorerProfilesQueryHandler : IRequestHandler<GetAllExplorerProfilesQuery, ApiResponse<PagedResult<GetExplorerDto>>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetAllExplorerProfilesQueryHandler> _logger;

        public GetAllExplorerProfilesQueryHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<GetAllExplorerProfilesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetExplorerDto>>> Handle(GetAllExplorerProfilesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all explorers - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Select(e => ExplorerProfileMapper.ToGetDto(e))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} profiles out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetExplorerDto>>.Success(result);
        }
    }
}
