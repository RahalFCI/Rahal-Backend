using Shared.Domain.Events;

namespace Shared.Application.Events.Users
{
    /// <summary>
    /// Published to RabbitMQ when a user unfollows another user.
    /// </summary>
    public record UserUnfollowedEvent(Guid FollowerId, Guid FollowingId, DateTime Timestamp) : BaseDomainEvent;
}
