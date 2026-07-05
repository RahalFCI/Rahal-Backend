using System.Text.Json;
using Notifications.Application.Extensions;
using Notifications.Application.DTOs;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;

namespace Notifications.Application.Mappers
{
    public static class NotificationDtoMapper
    {
        private const string DefaultActorName = "Someone";

        public static NotificationResponseDto Map(Notification notification, string? actorName = null)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                TargetId = notification.TargetId,
                Message = BuildMessage(notification, actorName),
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public static string BuildMessage(Notification notification, string? actorName = null)
        {
            var name = string.IsNullOrWhiteSpace(actorName) ? DefaultActorName : actorName;

            return notification.Type.ToNotificationType() switch
            {
                NotificationType.SocialPostLike => $"{name} liked your post.",
                NotificationType.SocialFollow => $"{name} started following you.",
                NotificationType.SocialPostComment => BuildCommentMessage(name, GetPreview(notification.Metadata)),
                NotificationType.SocialNewPost => BuildNewPostMessage(name, GetPreview(notification.Metadata)),
                _ => "You have a new notification."
            };
        }

        private static string BuildCommentMessage(string actorName, string? preview)
        {
            return string.IsNullOrWhiteSpace(preview)
                ? $"{actorName} commented on your post."
                : $"{actorName} commented: {preview}...";
        }

        private static string BuildNewPostMessage(string actorName, string? preview)
        {
            return string.IsNullOrWhiteSpace(preview)
                ? $"{actorName} added a new post."
                : $"{actorName} added a new post: {preview}...";
        }

        private static string? GetPreview(Dictionary<string, JsonElement>? metadata)
        {
            if (metadata is null ||
                !metadata.TryGetValue("preview", out var previewElement) ||
                previewElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return previewElement.GetString();
        }
    }
}
