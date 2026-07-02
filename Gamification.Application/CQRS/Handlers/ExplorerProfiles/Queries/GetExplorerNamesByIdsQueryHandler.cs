using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.Interfaces;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    // Lets modules outside Gamification (e.g. Places' check-ins and reviews,
    // which store an ExplorerProfile.UserId but have no relationship to the
    // ExplorerProfile table) resolve explorer display names in bulk without
    // taking a direct dependency on Gamification's schema.
    // Note: ExplorerProfile's primary key is UserId, not the inherited
    // BaseEntity.Id column (see ExplorerProfileConfiguration: HasKey(e =>
    // e.UserId)) - Id is vestigial and unused. Every "ExplorerId" scattered
    // across the codebase (UserStats.ExplorerProfileId, ExplorerAchievement.
    // ExplorerId, CheckIn.ExplorerId, etc.) actually stores UserId.
    public class GetExplorerNamesByIdsQueryHandler : IRequestHandler<GetExplorerNamesByIdsQuery, ApiResponse<List<ExplorerNameDto>>>
    {
        private readonly IGamificationRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetExplorerNamesByIdsQueryHandler> _logger;

        public GetExplorerNamesByIdsQueryHandler(
            IGamificationRepository<ExplorerProfile> repository,
            ILogger<GetExplorerNamesByIdsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ExplorerNameDto>>> Handle(GetExplorerNamesByIdsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resolving explorer names for {Count} ids", request.Ids.Count);

            var distinctIds = request.Ids.Distinct().ToList();

            var names = await _repository.GetTable()
                .Where(ep => distinctIds.Contains(ep.UserId))
                .Select(ep => new ExplorerNameDto { Id = ep.UserId, DisplayName = ep.DisplayName })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<ExplorerNameDto>>.Success(names);
        }
    }
}
