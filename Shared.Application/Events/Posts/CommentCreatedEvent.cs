using Shared.Domain.Events;

namespace Shared.Application.Events.Posts
{
    /// <summary>
    /// Published to RabbitMQ when a user comments on a post.
    /// </summary>
    public record CommentCreatedEvent(
        Guid CommentId,
        Guid PostId,
        Guid CommenterId,
        Guid PostAuthorId,
        string Preview,
        DateTime Timestamp) : BaseDomainEvent;
}
