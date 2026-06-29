using MassTransit;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Shared.Application.Events.Posts;
using Shared.Application.Events.SocialMedia;
using Shared.Application.Interfaces;
using Users.Contracts.Interfaces;

namespace Notifications.Application.EventConsumers
{
    public class PostCreatedConsumer : SocialNotificationConsumerBase, IConsumer<PostCreatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFcmNotificationService _fcmNotificationService;
        private readonly IUsersPublicApi _usersPublicApi;

        public PostCreatedConsumer(
            INotificationRepository notificationRepository,
            IFcmNotificationService fcmNotificationService,
            IUsersPublicApi usersPublicApi)
        {
            _notificationRepository = notificationRepository;
            _fcmNotificationService = fcmNotificationService;
            _usersPublicApi = usersPublicApi;
        }

        public async Task Consume(ConsumeContext<PostCreatedEvent> context)
        {
            if (!context.Headers.TryGetHeader(SocialEventHeaders.SocialFanoutCompleted, out _))
            {
                return;
            }

            var message = context.Message;
            var recipientIds = message.RecipientUserIds?
                .Where(userId => userId != Guid.Empty && userId != message.UserId)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (recipientIds.Count == 0)
            {
                return;
            }

            var createdAt = DateTime.UtcNow;
            var notifications = recipientIds
                .Select(userId => new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ActorId = message.UserId,
                    Type = "Social.NewPost",
                    TargetId = message.PostId.ToString(),
                    Metadata = CreatePreviewMetadata(message.ContentPreview),
                    IsRead = false,
                    CreatedAt = createdAt
                })
                .ToList();

            _notificationRepository.AddRange(notifications);
            await _notificationRepository.SaveChangesAsync(context.CancellationToken);

            var tokensByUserId = await _notificationRepository.GetFcmTokensByUserIdsAsync(
                recipientIds,
                context.CancellationToken);

            if (tokensByUserId.Count == 0)
            {
                return;
            }

            var actorName = await GetActorNameAsync(message.UserId, context.CancellationToken);

            foreach (var notification in notifications)
            {
                if (!tokensByUserId.TryGetValue(notification.UserId, out var token) ||
                    string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                await _fcmNotificationService.SendMulticastAsync(
                    new[] { token },
                    PushTitle,
                    $"{actorName} added a new post.",
                    CreatePushPayload(notification));
            }
        }

        private async Task<string> GetActorNameAsync(Guid actorId, CancellationToken cancellationToken)
        {
            var users = await _usersPublicApi.GetUsersDisplayNamesAsync(new[] { actorId }, cancellationToken);
            return users.FirstOrDefault()?.DisplayName ?? "Someone";
        }
    }

    public class PostCreatedConsumerDefinition : ConsumerDefinition<PostCreatedConsumer>
    {
        public PostCreatedConsumerDefinition()
        {
            EndpointName = "notifications-social-post-created";
        }
    }
}
