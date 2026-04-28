using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Domain.Events;
using Places.Infrastructure.Search;
using Places.Infrastructure.Persistence;
using Shared.Application.Interfaces;

namespace Places.Infrastructure.Search.EventHandlers
{
    public class PlaceCreatedEventHandler : INotificationHandler<PlaceCreatedEvent>
    {
        private readonly ISearchService<PlaceSearchDocument> _searchService;
        private readonly PlacesDbContext _dbContext;
        private readonly ILogger<PlaceCreatedEventHandler> _logger;

        public PlaceCreatedEventHandler(
            ISearchService<PlaceSearchDocument> searchService,
            PlacesDbContext dbContext,
            ILogger<PlaceCreatedEventHandler> logger)
        {
            _searchService = searchService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Handle(PlaceCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing PlaceCreatedEvent for place {PlaceId}", notification.PlaceId);

                var place = await _dbContext.Place
                    .Include(p => p.PlaceCategory)
                    .FirstOrDefaultAsync(p => p.Id == notification.PlaceId, cancellationToken);

                if (place == null)
                {
                    _logger.LogWarning("Place {PlaceId} not found when indexing after creation", notification.PlaceId);
                    return;
                }

                var searchDocument = place.ToSearchDocument();
                await _searchService.IndexAsync(searchDocument, cancellationToken);

                _logger.LogInformation("Successfully indexed place {PlaceId} in search service", notification.PlaceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred while indexing place {PlaceId} after creation. " +
                    "Place creation was successful but will not be searchable until next sync.",
                    notification.PlaceId);
            }
        }
    }
}
