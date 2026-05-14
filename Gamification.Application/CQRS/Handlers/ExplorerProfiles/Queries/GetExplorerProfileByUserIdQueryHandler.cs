using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Queries
{
    public class GetExplorerProfileByUserIdQueryHandler : IRequestHandler<GetExplorerProfileByUserIdQuery, ExplorerProfile?>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly ILogger<GetExplorerProfileByUserIdQueryHandler> _logger;

        public GetExplorerProfileByUserIdQueryHandler(
            IGenericRepository<ExplorerProfile> repository,
            ILogger<GetExplorerProfileByUserIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ExplorerProfile?> Handle(GetExplorerProfileByUserIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorer profile for user {UserId}", request.UserId);

            var profile = await _repository.GetTable()
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            return profile;
        }
    }
}
