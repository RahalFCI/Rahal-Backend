using MassTransit;
using Notifications.Application.Extensions;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Application.Events.SocialMedia;
using Shared.Application.Events.Users;
using Shared.Application.Interfaces;
using Users.Contracts.Interfaces;

namespace Notifications.Application.EventConsumers
{
    public class UserFollowedConsumer : SocialNotificationConsumerBase, IConsumer<UserFollowedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IFcmNotificationService _fcmNotificationService;
        private readonly IUsersPublicApi _usersPublicApi;

        public UserFollowedConsumer(
            INotificationRepository notificationRepository,
            IFcmNotificationService fcmNotificationService,
            IUsersPublicApi usersPublicApi)
        {
            _notificationRepository = notificationRepository;
            _fcmNotificationService = fcmNotificationService;
            _usersPublicApi = usersPublicApi;
        }

        public async Task Consume(ConsumeContext<UserFollowedEvent> context)
        {
            if (!context.Headers.TryGetHeader(SocialEventHeaders.SocialFanoutCompleted, out _))
            {
                return;
            }

            var message = context.Message;
            if (message.FollowerId == message.FollowingId)
            {
                return;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.FollowingId,
                ActorId = message.FollowerId,
                Type = NotificationType.SocialFollow.ToStoredValue(),
                TargetId = null,
                Metadata = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _notificationRepository.Add(notification);
            await _notificationRepository.SaveChangesAsync(context.CancellationToken);

            var token = await _notificationRepository.GetFcmTokenAsync(
                message.FollowingId,
                context.CancellationToken);

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var actorName = await GetActorNameAsync(message.FollowerId, context.CancellationToken);
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

    public class UserFollowedConsumerDefinition : ConsumerDefinition<UserFollowedConsumer>
    {
        public UserFollowedConsumerDefinition()
        {
            EndpointName = "notifications-social-user-followed";
        }
    }
}
