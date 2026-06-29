using System.Text.Json;

namespace Notifications.Application.DTOs
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid? ActorId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? TargetId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public Dictionary<string, JsonElement>? Metadata { get; set; }
    }
}
