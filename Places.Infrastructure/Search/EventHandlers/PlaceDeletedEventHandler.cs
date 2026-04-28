using MediatR;
using Microsoft.Extensions.Logging;
using Places.Domain.Events;
using Places.Infrastructure.Search;
using Shared.Application.Interfaces;

namespace Places.Infrastructure.Search.EventHandlers
{
    public class PlaceDeletedEventHandler : INotificationHandler<PlaceDeletedEvent>
    {
        private readonly ISearchService<PlaceSearchDocument> _searchService;
        private readonly ILogger<PlaceDeletedEventHandler> _logger;

        public PlaceDeletedEventHandler(
            ISearchService<PlaceSearchDocument> searchService,
            ILogger<PlaceDeletedEventHandler> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public async Task Handle(PlaceDeletedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing PlaceDeletedEvent for place {PlaceId}", notification.PlaceId);

                await _searchService.DeleteAsync(notification.PlaceId.ToString(), cancellationToken);

                _logger.LogInformation("Successfully deleted place {PlaceId} from search service", notification.PlaceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred while deleting place {PlaceId} from search service. " +
                    "Place deletion was successful but search index may still contain the place.",
                    notification.PlaceId);
            }
        }
    }
}
