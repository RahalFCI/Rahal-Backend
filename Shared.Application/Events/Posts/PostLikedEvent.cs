using Shared.Domain.Events;

namespace Shared.Application.Events.Posts
{
    /// <summary>
    /// Published to RabbitMQ when a user likes a post.
    /// Consumers can react (e.g., send notifications, update leaderboard)
    /// without coupling to the PostService.
    /// </summary>
    public record PostLikedEvent(Guid PostId, Guid LikerId, Guid PostAuthorId, DateTime Timestamp) : BaseDomainEvent;
}
