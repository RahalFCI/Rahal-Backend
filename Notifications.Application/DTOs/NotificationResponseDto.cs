namespace Notifications.Application.DTOs
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? TargetId { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
