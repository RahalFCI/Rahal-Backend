using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Domain.Enums;

namespace Notifications.Application.Services
{
    public class NotificationService : INotificationService
    {
        private const int DefaultLimit = 20;
        private const int MaxLimit = 100;

        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
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

            var responseNotifications = notifications
                .Select(Map)
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

        private static NotificationResponseDto Map(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                ActorId = notification.ActorId,
                Type = notification.Type,
                TargetId = notification.TargetId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                Metadata = notification.Metadata
            };
        }
    }
}
