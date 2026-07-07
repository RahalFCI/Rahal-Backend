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
    public class GetExplorerNamesByIdsQueryHandler : IRequestHandler<GetExplorerNamesByIdsQuery, ApiResponse<List<GetExplorerNameDto>>>
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

        public async Task<ApiResponse<List<GetExplorerNameDto>>> Handle(GetExplorerNamesByIdsQuery request, CancellationToken cancellationToken)
        {
            var ids = request.Ids.Distinct().ToArray();

            _logger.LogInformation("Fetching display names for {Count} explorers", ids.Length);

            // Other modules (Places check-ins/reviews) store the auth user id, which
            // maps to ExplorerProfile.UserId - not the profile's own primary key.
            var names = await _repository.GetTable()
                .AsNoTracking()
                .Where(e => ids.Contains(e.UserId))
                .Select(e => new GetExplorerNameDto(e.UserId, e.DisplayName))
                .ToListAsync(cancellationToken);

            return ApiResponse<List<GetExplorerNameDto>>.Success(names);
        }
    }
}
