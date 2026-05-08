using Gamification.Application.CQRS.Handlers.Challenges.Queries;
using Gamification.Application.CQRS.Queries.Challenge;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Challenge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetAllExplorerProfilesQueryHandler : IRequestHandler<GetAllExplorerProfilesQuery, ApiResponse<IEnumerable<GetExplorerDto>>>
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

        public async Task<ApiResponse<IEnumerable<GetExplorerDto>>> Handle(GetAllExplorerProfilesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all explorers");

            var explorerProfiles = await _repository.GetAllAsync(cancellationToken: cancellationToken);
            var dtos = ExplorerProfileMapper.ToGetDtos(explorerProfiles);

            _logger.LogInformation("Retrieved {Count} Profiles", explorerProfiles.Count());

            return ApiResponse<IEnumerable<GetExplorerDto>>.Success(dtos);
        }
    }
}
