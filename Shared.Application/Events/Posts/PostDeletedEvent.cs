using Shared.Domain.Events;

namespace Shared.Application.Events.Posts
{
    /// <summary>
    /// Published to RabbitMQ when a post is deleted.
    /// </summary>
    public record PostDeletedEvent(Guid PostId, Guid AuthorId, DateTime Timestamp) : BaseDomainEvent;
}
