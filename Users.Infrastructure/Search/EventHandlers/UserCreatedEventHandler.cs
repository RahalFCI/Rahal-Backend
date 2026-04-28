using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Events;

namespace Users.Infrastructure.Search.EventHandlers
{

    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly ISearchService<UserSearchDocument> _searchService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(
            ISearchService<UserSearchDocument> searchService,
            UserManager<User> userManager,
            ILogger<UserCreatedEventHandler> logger)
        {
            _searchService = searchService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing UserCreatedEvent for user {UserId}", notification.UserId);

                // Fetch the user from database
                var user = await _userManager.FindByIdAsync(notification.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found when indexing after creation", notification.UserId);
                    return;
                }

                // Map to search document and index
                var searchDocument = user.ToSearchDocument();
                await _searchService.IndexAsync(searchDocument, cancellationToken);

                _logger.LogInformation("Successfully indexed user {UserId} in search service", notification.UserId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - search index failure must not block user creation
                _logger.LogError(ex, 
                    "Error occurred while indexing user {UserId} after creation. " +
                    "User creation was successful but will not be searchable until next sync.",
                    notification.UserId);
            }
        }
    }
}
