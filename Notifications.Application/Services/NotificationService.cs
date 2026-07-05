using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Application.Mappers;
using Notifications.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using Users.Contracts.Interfaces;

namespace Notifications.Application.Services
{
    public class NotificationService : INotificationService
    {
        private const int DefaultLimit = 20;
        private const int MaxLimit = 100;

        private readonly INotificationRepository _notificationRepository;
        private readonly IUsersPublicApi _usersPublicApi;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUsersPublicApi usersPublicApi)
        {
            _notificationRepository = notificationRepository;
            _usersPublicApi = usersPublicApi;
        }

        public async Task<ApiResponse<UnreadCountResponse>> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<UnreadCountResponse>.Failure(ErrorCode.Unauthorized);
            }

            var count = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);

            return ApiResponse<UnreadCountResponse>.Success(new UnreadCountResponse
            {
                UnreadCount = count
            });
        }

        public async Task<ApiResponse<NotificationsPagedResponse>> GetUserNotificationsPaginatedAsync(
            Guid userId,
            DateTime? cursor = null,
            int limit = DefaultLimit,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<NotificationsPagedResponse>.Failure(ErrorCode.Unauthorized);
            }

            limit = NormalizeLimit(limit);
            var cursorDate = cursor?.ToUniversalTime() ?? DateTime.UtcNow;

            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId,
                cursorDate,
                limit,
                cancellationToken);

            var actorNamesById = await GetActorNamesByIdAsync(notifications, cancellationToken);

            var responseNotifications = notifications
                .Select(notification => NotificationDtoMapper.Map(
                    notification,
                    notification.ActorId is Guid actorId && actorNamesById.TryGetValue(actorId, out var actorName)
                        ? actorName
                        : null))
                .ToList();

            return ApiResponse<NotificationsPagedResponse>.Success(new NotificationsPagedResponse
            {
                Notifications = responseNotifications,
                NextCursor = responseNotifications.Count == 0
                    ? null
                    : responseNotifications[^1].CreatedAt
            });
        }

        public async Task<ApiResponse<string>> MarkAsReadAsync(
            Guid userId,
            Guid? notificationId = null,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<string>.Failure(ErrorCode.Unauthorized);
            }

            if (notificationId is null)
            {
                await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
                return ApiResponse<string>.Success("Notifications marked as read");
            }

            var notification = await _notificationRepository.GetByIdAsync(notificationId.Value, cancellationToken);
            if (notification is null || notification.UserId != userId)
            {
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse<string>.Success("Notification marked as read");
        }

        public async Task<ApiResponse<string>> SetFcmTokenAsync(
            Guid userId,
            string fcmToken,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return ApiResponse<string>.Failure(ErrorCode.Unauthorized);
            }

            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                return ApiResponse<string>.Failure(ErrorCode.ValidationError);
            }

            await _notificationRepository.UpsertFcmTokenAsync(
                userId,
                fcmToken.Trim(),
                cancellationToken);

            return ApiResponse<string>.Success("FCM token saved successfully");
        }

        private static int NormalizeLimit(int limit)
        {
            if (limit <= 0)
            {
                return DefaultLimit;
            }

            return Math.Min(limit, MaxLimit);
        }

        private async Task<Dictionary<Guid, string>> GetActorNamesByIdAsync(
            IReadOnlyCollection<Notification> notifications,
            CancellationToken cancellationToken)
        {
            var actorIds = notifications
                .Where(notification => notification.ActorId.HasValue)
                .Select(notification => notification.ActorId!.Value)
                .Distinct()
                .ToList();

            if (actorIds.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            var users = await _usersPublicApi.GetUsersDisplayNamesAsync(actorIds, cancellationToken);
            return users.ToDictionary(user => user.Id, user => user.DisplayName);
        }
    }
}
