using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetExplorerProfilesIncludingDeletedQueryHandler : IRequestHandler<GetExplorerProfilesIncludingDeletedQuery, ApiResponse<IEnumerable<GetExplorerDto>>>
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

        public async Task<ApiResponse<IEnumerable<GetExplorerDto>>> Handle(GetExplorerProfilesIncludingDeletedQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all explorers");

            var explorerProfiles = await _repository.GetTable().IgnoreQueryFilters().ToListAsync(cancellationToken: cancellationToken);
            var dtos = ExplorerProfileMapper.ToGetDtos(explorerProfiles);

            _logger.LogInformation("Retrieved {Count} Profiles", explorerProfiles.Count());

            return ApiResponse<IEnumerable<GetExplorerDto>>.Success(dtos);
        }
    }
}
