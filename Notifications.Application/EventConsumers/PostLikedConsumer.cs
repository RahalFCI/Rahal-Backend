using MassTransit;
using Notifications.Application.Extensions;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Application.Events.Posts;
using Shared.Application.Interfaces;
using Users.Contracts.Interfaces;

namespace Notifications.Application.EventConsumers
{
    public class PostLikedConsumer : SocialNotificationConsumerBase, IConsumer<PostLikedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFcmNotificationService _fcmNotificationService;
        private readonly IUsersPublicApi _usersPublicApi;

        public PostLikedConsumer(
            INotificationRepository notificationRepository,
            IFcmNotificationService fcmNotificationService,
            IUsersPublicApi usersPublicApi)
        {
            _notificationRepository = notificationRepository;
            _fcmNotificationService = fcmNotificationService;
            _usersPublicApi = usersPublicApi;
        }

        public async Task Consume(ConsumeContext<PostLikedEvent> context)
        {
            var message = context.Message;
            if (message.LikerId == message.PostAuthorId)
            {
                return;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.PostAuthorId,
                ActorId = message.LikerId,
                Type = NotificationType.SocialPostLike.ToStoredValue(),
                TargetId = message.PostId.ToString(),
                Metadata = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepository.Add(notification);
            await _notificationRepository.SaveChangesAsync(context.CancellationToken);

            var token = await _notificationRepository.GetFcmTokenAsync(
                message.PostAuthorId,
                context.CancellationToken);

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var actorName = await GetActorNameAsync(message.LikerId, context.CancellationToken);
            var body = BuildPushMessage(notification, actorName);

            await _fcmNotificationService.SendMulticastAsync(
                new[] { token },
                PushTitle,
                body,
                CreatePushPayload(notification, actorName));
        }

        private async Task<string> GetActorNameAsync(Guid actorId, CancellationToken cancellationToken)
        {
            var users = await _usersPublicApi.GetUsersDisplayNamesAsync(new[] { actorId }, cancellationToken);
            return users.FirstOrDefault()?.DisplayName ?? "Someone";
        }
    }

    public class PostLikedConsumerDefinition : ConsumerDefinition<PostLikedConsumer>
    {
        public PostLikedConsumerDefinition()
        {
            EndpointName = "notifications-social-post-liked";
        }
    }
}
