using System.Text.Json;
using Notifications.Application.DTOs;
using Notifications.Domain.Entities;

namespace Notifications.Application.EventConsumers
{
    public abstract class SocialNotificationConsumerBase
    {
        protected const string PushTitle = "Rahal";

        protected static Dictionary<string, JsonElement>? CreatePreviewMetadata(string? preview)
        {
            if (string.IsNullOrWhiteSpace(preview))
            {
                return null;
            }

            return new Dictionary<string, JsonElement>
            {
                ["preview"] = JsonSerializer.SerializeToElement(preview)
            };
        }

        protected static Dictionary<string, string> CreatePushPayload(Notification notification)
        {
            var dto = Map(notification);
            var payload = new Dictionary<string, string>
            {
                ["Notification"] = JsonSerializer.Serialize(dto),
                ["NotificationId"] = notification.Id.ToString(),
                ["Type"] = notification.Type,
                ["UserId"] = notification.UserId.ToString()
            };

            if (notification.ActorId is not null)
            {
                payload["ActorId"] = notification.ActorId.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(notification.TargetId))
            {
                payload["TargetId"] = notification.TargetId;
            }

            return payload;
        }

        protected static NotificationResponseDto Map(Notification notification)
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
