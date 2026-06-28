using Notifications.Domain.Entities;

namespace Notifications.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId,
            DateTime cursor,
            int limit,
            CancellationToken cancellationToken = default);

        Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

        Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task UpsertFcmTokenAsync(Guid userId, string fcmToken, CancellationToken cancellationToken = default);
    }
}
