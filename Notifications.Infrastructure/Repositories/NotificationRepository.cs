using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationsDbContext _context;

        public NotificationRepository(NotificationsDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId && !notification.IsRead)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId,
            DateTime cursor,
            int limit,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId)
                .Where(notification => notification.CreatedAt < cursor)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<Notification?> GetByIdAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(notification => notification.Id == notificationId, cancellationToken);
        }

        public void Add(Notification notification)
        {
            _context.Notifications.Add(notification);
        }

        public void AddRange(IEnumerable<Notification> notifications)
        {
            _context.Notifications.AddRange(notifications);
        }

        public async Task<string?> GetFcmTokenAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserNotificationTokens
                .AsNoTracking()
                .Where(token => token.UserId == userId)
                .Select(token => token.FcmToken)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, string>> GetFcmTokensByUserIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            return await _context.UserNotificationTokens
                .AsNoTracking()
                .Where(token => userIds.Contains(token.UserId))
                .ToDictionaryAsync(token => token.UserId, token => token.FcmToken, cancellationToken);
        }

        public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await _context.Notifications
                .Where(notification => notification.UserId == userId && !notification.IsRead)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(notification => notification.IsRead, true),
                    cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpsertFcmTokenAsync(
            Guid userId,
            string fcmToken,
            CancellationToken cancellationToken = default)
        {
            var existingToken = await _context.UserNotificationTokens
                .FirstOrDefaultAsync(token => token.UserId == userId, cancellationToken);

            if (existingToken is null)
            {
                _context.UserNotificationTokens.Add(new UserNotificationToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FcmToken = fcmToken,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingToken.FcmToken = fcmToken;
                existingToken.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
