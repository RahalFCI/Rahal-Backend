using Shared.Domain.Events;

namespace Shared.Application.Events.Users
{
    /// <summary>
    /// Published to RabbitMQ when a user follows another user.
    /// </summary>
    public record UserFollowedEvent(Guid FollowerId, Guid FollowingId, DateTime Timestamp) : BaseDomainEvent;
}
