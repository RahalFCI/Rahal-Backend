using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Domain.Events;
using Places.Infrastructure.Search;
using Places.Infrastructure.Persistence;
using Shared.Application.Interfaces;

namespace Places.Infrastructure.Search.EventHandlers
{
    public class PlaceUpdatedEventHandler : INotificationHandler<PlaceUpdatedEvent>
    {
        private readonly ISearchService<PlaceSearchDocument> _searchService;
        private readonly PlacesDbContext _dbContext;
        private readonly ILogger<PlaceUpdatedEventHandler> _logger;

        public PlaceUpdatedEventHandler(
            ISearchService<PlaceSearchDocument> searchService,
            PlacesDbContext dbContext,
            ILogger<PlaceUpdatedEventHandler> logger)
        {
            _searchService = searchService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(PlaceUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing PlaceUpdatedEvent for place {PlaceId}", notification.PlaceId);

                var place = await _dbContext.Place
                    .Include(p => p.PlaceCategory)
                    .FirstOrDefaultAsync(p => p.Id == notification.PlaceId, cancellationToken);

                if (place == null)
                {
                    _logger.LogWarning("Place {PlaceId} not found when updating search index", notification.PlaceId);
                    return;
                }

                var searchDocument = place.ToSearchDocument();
                await _searchService.IndexAsync(searchDocument, cancellationToken);

                _logger.LogInformation("Successfully updated place {PlaceId} in search service", notification.PlaceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred while updating place {PlaceId} in search service. " +
                    "Place update was successful but search index may be stale.",
                    notification.PlaceId);
            }
        }
    }
}
