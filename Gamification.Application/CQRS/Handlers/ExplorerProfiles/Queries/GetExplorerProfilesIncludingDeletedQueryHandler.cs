using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
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

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetExplorerProfilesIncludingDeletedQueryHandler : IRequestHandler<GetExplorerProfilesIncludingDeletedQuery, ApiResponse<PagedResult<GetExplorerDto>>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetExplorerProfilesIncludingDeletedQueryHandler> _logger;

        public GetExplorerProfilesIncludingDeletedQueryHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<GetExplorerProfilesIncludingDeletedQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetExplorerDto>>> Handle(GetExplorerProfilesIncludingDeletedQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all explorers including deleted - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .IgnoreQueryFilters()
                .Select(e => ExplorerProfileMapper.ToGetDto(e))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} profiles out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetExplorerDto>>.Success(result);
        }
    }
}
