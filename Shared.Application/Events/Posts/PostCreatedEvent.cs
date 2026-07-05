using Shared.Domain.Events;

namespace Shared.Application.Events.Posts
{
    /// <summary>
    /// Published to RabbitMQ after a post is persisted. Consumers can react
    /// (e.g., fan-out to followers, update leaderboard) without coupling to PostService.
    /// </summary>
    public record PostCreatedEvent(
        Guid PostId,
        Guid UserId,
        DateTime CreatedAt,
        string? ContentPreview = null,
        List<Guid>? RecipientUserIds = null) : BaseDomainEvent;
}
