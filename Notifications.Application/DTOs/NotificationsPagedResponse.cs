namespace Notifications.Application.DTOs
{
    public class NotificationsPagedResponse
    {
        public List<NotificationResponseDto> Notifications { get; set; } = new();

        public DateTime? NextCursor { get; set; }
    }
}
