using System.Text.Json;
using Notifications.Domain.Entities;
using Notifications.Application.Mappers;

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

        protected static Dictionary<string, string> CreatePushPayload(
            Notification notification,
            string? actorName)
        {
            return new Dictionary<string, string>
            {
                ["Notification"] = JsonSerializer.Serialize(NotificationDtoMapper.Map(notification, actorName))
            };
        }

        protected static string BuildPushMessage(Notification notification, string? actorName)
        {
            return NotificationDtoMapper.BuildMessage(notification, actorName);
        }
    }
}
