using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Users.Domain.Events;

namespace Users.Infrastructure.Search.EventHandlers
{

    public class UserDeletedEventHandler : INotificationHandler<UserDeletedEvent>
    {
        private readonly ISearchService<UserSearchDocument> _searchService;
        private readonly ILogger<UserDeletedEventHandler> _logger;

        public UserDeletedEventHandler(
            ISearchService<UserSearchDocument> searchService,
            ILogger<UserDeletedEventHandler> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }


        public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing UserDeletedEvent for user {UserId}", notification.UserId);

                // Delete the user from search index
                await _searchService.DeleteAsync(notification.UserId.ToString(), cancellationToken);

                _logger.LogInformation("Successfully deleted user {UserId} from search service", notification.UserId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - search index failure must not block user deletion
                _logger.LogError(ex,
                    "Error occurred while deleting user {UserId} from search service. " +
                    "User deletion was successful but search index may still contain the user.",
                    notification.UserId);
            }
        }
    }
}
