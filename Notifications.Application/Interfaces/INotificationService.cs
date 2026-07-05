using Notifications.Application.DTOs;
using Shared.Application.DTOs;

namespace Notifications.Application.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<UnreadCountResponse>> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<NotificationsPagedResponse>> GetUserNotificationsPaginatedAsync(
            Guid userId,
            DateTime? cursor = null,
            int limit = 20,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> MarkAsReadAsync(
            Guid userId,
            Guid? notificationId = null,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> SetFcmTokenAsync(
            Guid userId,
            string fcmToken,
            CancellationToken cancellationToken = default);
    }
}
