using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Events;

namespace Users.Infrastructure.Search.EventHandlers
{

    public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
    {
        private readonly ISearchService<UserSearchDocument> _searchService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserUpdatedEventHandler> _logger;

        public UserUpdatedEventHandler(
            ISearchService<UserSearchDocument> searchService,
            UserManager<User> userManager,
            ILogger<UserUpdatedEventHandler> logger)
        {
            _searchService = searchService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing UserUpdatedEvent for user {UserId}", notification.UserId);

                // Fetch the updated user from database
                var user = await _userManager.FindByIdAsync(notification.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found when updating search index", notification.UserId);
                    return;
                }

                // Map to search document and index (upsert - replaces existing if present)
                var searchDocument = user.ToSearchDocument();
                await _searchService.IndexAsync(searchDocument, cancellationToken);

                _logger.LogInformation("Successfully updated user {UserId} in search service", notification.UserId);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - search index failure must not block user updates
                _logger.LogError(ex,
                    "Error occurred while updating user {UserId} in search service. " +
                    "User update was successful but search index may be stale.",
                    notification.UserId);
            }
        }
    }
}
