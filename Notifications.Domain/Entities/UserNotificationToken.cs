using Shared.Domain.Entities;

namespace Notifications.Domain.Entities
{
    public class UserNotificationToken : BaseEntity
    {
        public Guid UserId { get; set; }

        public string FcmToken { get; set; } = string.Empty;
    }
}
